# Deployment Guide

Deploy the **API** to Cloud Run and the **webapp** to Firebase Hosting.

> **Cost note:** Cloud Run and Firebase Hosting have generous always-free tiers.
> Cloud SQL (PostgreSQL) is **not free** — it consumes your $300 GCP trial credit.
> Everything else (Artifact Registry, Firestore, Pub/Sub) is on the always-free tier.

All shell commands are **PowerShell** (Windows PowerShell or PowerShell 7+).

---

## Prerequisites

- [gcloud CLI](https://cloud.google.com/sdk/docs/install)
- [Podman](https://podman.io/getting-started/installation)
- [firebase-tools](https://firebase.google.com/docs/cli): `npm install -g firebase-tools`
- [Node.js 18+](https://nodejs.org/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

---

## Phase 0 — Register your GCP Account

**Yes, you must click "Start free"** before you can create any GCP resources.

1. Go to [console.cloud.google.com](https://console.cloud.google.com)
2. Click **Start free** → enter your country, agree to terms, add a credit card
3. You receive **$300 in free credits** valid for 90 days

> **Your card will NOT be charged** during the free trial. GCP only suspends services (never auto-charges) when credits run out. You must manually upgrade to a paid account to be billed.

### Can I remove my credit card?

No — GCP requires a payment method on any active billing account. You have two safe options:

- **Leave it on file** — safe, no charge occurs during the trial or after (services just stop)
- **Close the billing account** — go to **Billing → Account management → Close billing account**. This removes the payment method but stops all services immediately.

✅ **Verification:** Go to **Billing → Overview** — confirm your $300 credit balance is shown.

---

## Phase 1 — GCP Infrastructure

### 1.1 Create a GCP Project

1. Click the project dropdown → **New Project**
2. Name it (e.g., `storage-mgmt`) and note the **Project ID**
3. Click **Create**

✅ **Verification:**
```powershell
gcloud projects list
# Your new project should appear in the list
```

### 1.2 Enable Required APIs

```powershell
gcloud auth login
gcloud config set project YOUR_PROJECT_ID

gcloud services enable `
  run.googleapis.com `
  artifactregistry.googleapis.com `
  sqladmin.googleapis.com `
  secretmanager.googleapis.com `
  firestore.googleapis.com `
  pubsub.googleapis.com `
  storage.googleapis.com `
  iamcredentials.googleapis.com
```

✅ **Verification:**
```powershell
gcloud services list --enabled --filter="name:(run OR artifactregistry OR sqladmin OR secretmanager OR firestore OR pubsub OR storage OR iamcredentials)"
# All 8 services should appear as ENABLED
```

### 1.3 Create Artifact Registry Repository

1. Go to **Artifact Registry → Repositories → Create Repository**
2. Set: Name `storage-api`, Format `Docker`, Region e.g. `asia-southeast1`
3. Click **Create**

✅ **Verification:**
```powershell
gcloud artifacts repositories list --location=YOUR_REGION
# storage-api should appear
```

### 1.4 Create Cloud SQL Instance (PostgreSQL)

> ✅ Use the **free trial instance** — 30 days free, no cost for the instance itself (Enterprise Plus, PostgreSQL 18, 8 vCPUs, 64GB RAM, 100GB storage). One free trial per project.

1. Go to **SQL → Create Instance → Choose PostgreSQL** — GCP shows the free trial offer automatically
2. On the free trial screen:
   - **Database Engine:** PostgreSQL (already set)
   - **Instance ID:** change to `storage-db`
   - **Password:** click **Generate** and **save the password** — this is for the `postgres` admin user
   - **Region:** change to match your Cloud Run region (e.g., `asia-southeast1`)
3. Click **Create free instance** (~5 min)
4. Once ready: **Databases → Create Database** → name `storagemanagement`
5. **Users → Add User Account** → name `appuser`, set a password

Note the **Connection name** on the instance overview page (format: `PROJECT:REGION:INSTANCE`).

> ⚠️ **After 30 days:** the instance stops serving requests but your data is kept for a 90-day grace period before deletion. You will not be charged — just upgrade or delete it before the grace period ends.
> **Charged for:** data transfer outside the instance region and the final backup when you delete. Keep Cloud Run in the **same region** as Cloud SQL to avoid transfer costs.

✅ **Verification:**
```powershell
gcloud sql instances list
# storage-db should show RUNNABLE status
```

### 1.4.1 Connect to Cloud SQL from pgAdmin

Two options. **Option A (Auth Proxy)** is recommended — no internet exposure.

**Option A — Cloud SQL Auth Proxy**

```powershell
# 1. Download the proxy
Invoke-WebRequest `
  -Uri "https://storage.googleapis.com/cloud-sql-connectors/cloud-sql-proxy/v2.15.2/cloud-sql-proxy.x64.exe" `
  -OutFile "cloud-sql-proxy.exe"

# 2. Run the proxy (keep this terminal open)
.\cloud-sql-proxy.exe YOUR_PROJECT_ID:YOUR_REGION:storage-db --port=5432
```

Connect pgAdmin to:

| Field | Value |
|---|---|
| Host | `127.0.0.1` |
| Port | `5432` |
| Database | `storagemanagement` |
| Username | `postgres` |
| Password | password saved during setup |
| SSL mode | `disable` (proxy handles encryption) |

**Option B — Public IP + Authorized Network**

```powershell
# Get your current public IP
(Invoke-RestMethod "https://api.ipify.org")
```

Then go to **Cloud SQL → storage-db → Edit → Connections → Add a network**, paste your IP.

Connect pgAdmin to:

| Field | Value |
|---|---|
| Host | instance public IP (shown on Cloud SQL overview) |
| Port | `5432` |
| Database | `storagemanagement` |
| Username | `postgres` |
| Password | password saved during setup |
| SSL mode | `require` |

> ⚠️ Option B exposes the database on your IP. If your IP changes, update the authorized network again.

### 1.4.2 Run Database Migrations

The `InitialCreate` migration creates the `storagedocument` table. Run it once against Cloud SQL before deploying the API.

Start the Auth Proxy (Option A from 1.4.1) if it isn't already running, then in a separate terminal:

```powershell
Set-Location srcs/app/dal/storagedal

dotnet ef database update `
  --connection "Host=127.0.0.1;Port=5432;Database=storagemanagement;Username=postgres;Password=YOUR_POSTGRES_PASSWORD"
```

> Run as `postgres` (admin) — `appuser` does not have CREATE TABLE rights. The `--connection` flag overrides the connection string for this command only; your config files are not changed.

Then grant `appuser` the permissions it needs at runtime. In pgAdmin, open the **Query Tool** against `storagemanagement` and run:

```sql
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE storagedocument TO appuser;
GRANT USAGE, SELECT ON SEQUENCE storagedocument_id_seq TO appuser;
```

✅ **Verification:** In pgAdmin, open **storagemanagement → Schemas → public → Tables** — the `storagedocument` table should appear, and `appuser` should be listed under its privileges.

### 1.5 Create Cloud Storage Bucket

1. Go to **Cloud Storage → Buckets → Create**
2. **Name:** `YOUR_PROJECT_ID-documents`
3. **Region:** select the same region as Cloud Run (e.g., `asia-southeast1`)
4. Leave all other settings as default → click **Create**

✅ **Verification:**
```powershell
gcloud storage buckets list
# gs://YOUR_PROJECT_ID-documents/ should appear
```

**Configure CORS** so browsers can PUT files directly to the bucket via signed URLs.

Open **Cloud Shell** from the GCP Console (the `>_` icon, top right) and run:

```bash
cat > cors.json << 'EOF'
[{"origin":["https://YOUR_PROJECT_ID.web.app"],"method":["PUT","GET","HEAD","OPTIONS"],"responseHeader":["Content-Type"],"maxAgeSeconds":3600}]
EOF

gsutil cors set cors.json gs://YOUR_PROJECT_ID-documents
```

> Use Cloud Shell rather than a local terminal — `gcloud storage buckets update --cors-file` silently ignores files written with a UTF-8 BOM (the default on Windows PowerShell), and local `gsutil` may have the same path issue.

✅ **Verification:**
```bash
gsutil cors get gs://YOUR_PROJECT_ID-documents
# Should print the CORS rule with your web.app origin
```

### 1.6 Store Secrets in Secret Manager

Go to **Secret Manager → Create Secret** and create:

| Secret name | Value |
|---|---|
| `db-connection-string` | `Host=/cloudsql/YOUR_PROJECT_ID:YOUR_REGION:storage-db;Database=storagemanagement;Username=appuser;Password=YOUR_APPUSER_PASSWORD` |

✅ **Verification:**
```powershell
gcloud secrets list
# db-connection-string and db-password should appear
```

### 1.7 Create a Service Account for Cloud Run

1. Go to **IAM & Admin → Service Accounts → Create Service Account**
2. Name: `storage-api-sa`
3. Grant roles: Cloud Datastore User, Pub/Sub Publisher, Storage Object Admin, Cloud SQL Client, Secret Manager Secret Accessor, Service Account Token Creator
4. Click **Done**

> **Why Service Account Token Creator?** The API uses this service account to sign Cloud Storage URLs (`SignedUrlGenerator`). Without this role the service account cannot call `iam.serviceAccounts.signBlob` on itself, resulting in a 403 from `iamcredentials.googleapis.com`.

✅ **Verification:**
```powershell
gcloud iam service-accounts list
# storage-api-sa@YOUR_PROJECT_ID.iam.gserviceaccount.com should appear
```

### 1.8 Create Pub/Sub Topics and Subscription

**Create the main topic:**

1. Go to **Pub/Sub → Topics → Create Topic**
2. **Topic ID:** `document-events`
3. Click **Create**

**Create the dead-letter topic:**

1. Click **Create Topic** again
2. **Topic ID:** `document-events-dlq`
3. Click **Create**

**Create the subscription:**

1. Go to **Pub/Sub → Subscriptions → Create Subscription**
2. **Subscription ID:** `document-processor-sub`
3. **Select a Cloud Pub/Sub topic:** `document-events`
4. **Dead lettering:** enable → select `document-events-dlq`
5. Leave all other settings as default → click **Create**

✅ **Verification:**
```powershell
gcloud pubsub topics list
# projects/YOUR_PROJECT_ID/topics/document-events and document-events-dlq should appear

gcloud pubsub subscriptions list
# projects/YOUR_PROJECT_ID/subscriptions/document-processor-sub should appear
```

---

## Phase 2 — Seed Firestore Data

### Via Firebase Console (recommended)

1. Go to [console.firebase.google.com](https://console.firebase.google.com) → **Add project** → select your GCP project
2. Go to **Firestore Database → Create database**
3. **Step 1 - Select edition:** choose **Standard edition** → click **Next**
4. **Step 2 - Database ID & location:** leave ID as `(default)`, select your region → click **Next**
5. **Step 3 - Configure:** choose **Production mode** (locked down rules)
6. Click **Create** — then click **Start collection**, name it `document-types`

✅ **Verification:** Go to **Firebase Console → Firestore Database** — the `document-types` collection

---

## Phase 3 — Build & Push the API Image

### 3.1 Authenticate Podman with Artifact Registry

```powershell
podman login `
  -u oauth2accesstoken `
  --password=$(gcloud auth print-access-token) `
  YOUR_REGION-docker.pkg.dev
```

> The token expires in ~1 hour. Re-run if you get auth errors during push.

✅ **Verification:**
```powershell
podman login --get-login YOUR_REGION-docker.pkg.dev
# Should print: oauth2accesstoken
```

### 3.2 Build the Image

> Run from the **repo root**. The build context must be `srcs/app/` because the API references sibling projects.

```powershell
podman build `
  -f srcs/app/api/Dockerfile `
  srcs/app/ `
  -t YOUR_REGION-docker.pkg.dev/YOUR_PROJECT_ID/storage-api/storage-api:latest
```

✅ **Verification:**
```powershell
podman images
# storage-api:latest should appear in the list
```

### 3.3 Push the Image

```powershell
podman push YOUR_REGION-docker.pkg.dev/YOUR_PROJECT_ID/storage-api/storage-api:latest
```

✅ **Verification:**
```powershell
gcloud artifacts docker images list YOUR_REGION-docker.pkg.dev/YOUR_PROJECT_ID/storage-api
# storage-api image with latest tag should appear
```

---

## Phase 4 — Deploy API to Cloud Run

Choose **Option A** (public, simpler) or **Option B** (private, more secure). You can switch between them at any time.

### Option A — Public Access

The API has a public HTTPS URL. Anyone with the URL can call it (no auth required).

```powershell
gcloud run deploy storage-api `
  --image=YOUR_REGION-docker.pkg.dev/YOUR_PROJECT_ID/storage-api/storage-api:latest `
  --region=YOUR_REGION `
  --service-account=storage-api-sa@YOUR_PROJECT_ID.iam.gserviceaccount.com `
  --add-cloudsql-instances=YOUR_PROJECT_ID:YOUR_REGION:storage-db `
  --ingress=all `
  --allow-unauthenticated `
  --set-env-vars="GCP__ProjectId=YOUR_PROJECT_ID,GCP__StorageBucket=YOUR_PROJECT_ID-documents,GCP__PubSubTopic=document-events,GCP__FirestoreDatabase=(default),FrontUrl=https://YOUR_PROJECT_ID.web.app" `
  --set-secrets="Storage__PostgresDb__ConnectionString=db-connection-string:latest" `
  --port=8080 `
  --project=YOUR_PROJECT_ID
```

After deploy, gcloud prints the service URL (`https://storage-api-HASH-REGION.a.run.app`). Save it.

✅ **Verification:**
```powershell
$SERVICE_URL = "https://storage-api-HASH-REGION.a.run.app"
Invoke-RestMethod "$SERVICE_URL/scalar/v1"
# Should return HTTP 200
```

---

### Option B — Private Access (internal only)

The API has **no public URL**. External internet traffic is rejected at the network level. Access is only possible via a local gcloud proxy tunnel authenticated with your identity.

```powershell
gcloud run deploy storage-api `
  --image=YOUR_REGION-docker.pkg.dev/YOUR_PROJECT_ID/storage-api/storage-api:latest `
  --region=YOUR_REGION `
  --service-account=storage-api-sa@YOUR_PROJECT_ID.iam.gserviceaccount.com `
  --add-cloudsql-instances=YOUR_PROJECT_ID:YOUR_REGION:storage-db `
  --ingress=internal `
  --no-allow-unauthenticated `
  --set-env-vars="GCP__ProjectId=YOUR_PROJECT_ID,GCP__StorageBucket=YOUR_PROJECT_ID-documents,GCP__PubSubTopic=document-events,GCP__FirestoreDatabase=(default),FrontUrl=https://YOUR_PROJECT_ID.web.app" `
  --set-secrets="Storage__PostgresDb__ConnectionString=db-connection-string:latest" `
  --port=8080 `
  --project=YOUR_PROJECT_ID
```

Start the local proxy tunnel to access it:

```powershell
gcloud run services proxy storage-api `
  --region=YOUR_REGION `
  --port=8080 `
  --project=YOUR_PROJECT_ID
```

This tunnels through Google's infrastructure using your gcloud credentials. Keep the terminal open.

✅ **Verification (with proxy running):**
```powershell
Invoke-RestMethod "http://localhost:8080/scalar/v1"
# Should return HTTP 200
```

---

### Switching from Public to Private (or back)

No redeployment of the image is needed — just update the two flags:

**Public → Private:**
```powershell
gcloud run services update storage-api `
  --region=YOUR_REGION `
  --ingress=internal `
  --no-allow-unauthenticated
```

**Private → Public:**
```powershell
gcloud run services update storage-api `
  --region=YOUR_REGION `
  --ingress=all `
  --allow-unauthenticated
```

✅ **Verification:** Run the relevant verification step from Option A or B above after switching.

---

## Phase 5 — Deploy webapp to Firebase Hosting

### 5.1 Link Firebase to your Project

```powershell
Set-Location srcs/app/webapp
firebase login
firebase use --add
# Select your GCP project and give it an alias e.g. "production"
```

✅ **Verification:**
```powershell
firebase projects:list
# Your project should appear with alias "production"
```

### 5.2 Set the API URL for Production

Create `srcs/app/webapp/.env.production`:

**If using Option A (public):**
```env
VITE_API_BASE_URL=https://storage-api-HASH-REGION.a.run.app/
VITE_ENVIRONMENT=production
```

**If using Option B (private):**
```env
VITE_API_BASE_URL=http://localhost:8080/api
VITE_ENVIRONMENT=production
```

### 5.3 Build and Deploy

```powershell
Set-Location srcs/app/webapp
npm install
npm run build
firebase deploy --only hosting
```

✅ **Verification:**
```powershell
# Firebase prints the URL after deploy. Open it in your browser:
Start-Process "https://YOUR_PROJECT_ID.web.app"
# The webapp should load and be able to fetch document types
```

---

## Daily Development Workflow

### Option A — Public API

The API is always reachable. No tunnel needed.

```powershell
# Open the webapp in browser
Start-Process "https://YOUR_PROJECT_ID.web.app"

# Or run locally against the public API
Set-Location srcs/app/webapp
npm run dev
```

### Option B — Private API

Must start the proxy tunnel first, then use the app.

```powershell
# Terminal 1 — keep open (Ctrl+C to stop)
gcloud run services proxy storage-api --region=YOUR_REGION --port=8080

# Terminal 2 — open the webapp
Start-Process "https://YOUR_PROJECT_ID.web.app"
# Or run locally:
Set-Location srcs/app/webapp; npm run dev
```

---

## Re-deploying After Code Changes

```powershell
# Re-authenticate Podman if token expired (~1 hour)
podman login -u oauth2accesstoken --password=$(gcloud auth print-access-token) YOUR_REGION-docker.pkg.dev

# Rebuild and push (from repo root)
podman build -f srcs/app/api/Dockerfile srcs/app/ -t YOUR_REGION-docker.pkg.dev/YOUR_PROJECT_ID/storage-api/storage-api:latest
podman push YOUR_REGION-docker.pkg.dev/YOUR_PROJECT_ID/storage-api/storage-api:latest

# Redeploy
gcloud run deploy storage-api `
  --image=YOUR_REGION-docker.pkg.dev/YOUR_PROJECT_ID/storage-api/storage-api:latest `
  --region=YOUR_REGION
```

**webapp:**
```powershell
Set-Location srcs/app/webapp
npm run build
firebase deploy --only hosting
```

---

## Tear Down — Remove All Resources to Avoid Costs

Run these to delete every resource created in this guide. **Irreversible.**

```powershell
$PROJECT = "YOUR_PROJECT_ID"
$REGION  = "YOUR_REGION"

# 1. Cloud Run service
gcloud run services delete storage-api --region=$REGION --project=$PROJECT --quiet

# 2. Cloud SQL (most expensive — delete first)
gcloud sql instances delete storage-db --project=$PROJECT --quiet

# 3. Artifact Registry
gcloud artifacts repositories delete storage-api --location=$REGION --project=$PROJECT --quiet

# 4. Secrets
gcloud secrets delete db-connection-string --project=$PROJECT --quiet
gcloud secrets delete db-password --project=$PROJECT --quiet

# 5. Cloud Storage bucket
gsutil rm -r gs://$PROJECT-documents

# 6. Pub/Sub
gcloud pubsub subscriptions delete document-processor-sub --project=$PROJECT --quiet
gcloud pubsub topics delete document-events --project=$PROJECT --quiet
gcloud pubsub topics delete document-events-dlq --project=$PROJECT --quiet

# 7. Firebase Hosting
firebase hosting:disable --project=$PROJECT

# 8. Service account
gcloud iam service-accounts delete storage-api-sa@$PROJECT.iam.gserviceaccount.com --project=$PROJECT --quiet

# 9. Delete entire project (removes Firestore, billing link, and credit card requirement)
gcloud projects delete $PROJECT --quiet
```

> Step 9 (delete project) is the only way to fully remove the credit card requirement.
> To just pause costs without deleting everything, only run step 2 (Cloud SQL is the sole paid resource).

### Remove Your Credit Card

Once all resources are torn down:

1. Go to **Billing → Account management**
2. Click **Close billing account**
3. Your credit card is removed automatically when the billing account is closed
