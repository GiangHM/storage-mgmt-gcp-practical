# Local Development Setup

Build and run the Storage Management app locally with GCP emulators (Firestore, Pub/Sub) and Testcontainers (PostgreSQL).

## Prerequisites

### Required

- **.NET 10 SDK** – [Download](https://dotnet.microsoft.com/download)
- **Node.js 18+** – [Download](https://nodejs.org/)
- **Python 3.9+** – [Download](https://www.python.org/downloads/)
- **Google Cloud SDK** – [Install gcloud CLI](https://cloud.google.com/sdk/docs/install)
- **Docker Desktop** – Required for GCP emulators & Testcontainers
- **VS Code** with extensions: C# Dev Kit, Cloud Code, Vue Official

### Recommended

- **Postman** or **REST Client** – API testing
- **firebase-tools** – Firestore emulator management (`npm install -g firebase-tools`)
- **draw.io** – View architecture diagrams

## Configuration

### Backend API (`app/api/`)

Create `appsettings.Development.json`:

```json
{
  "GCP": {
    "ProjectId": "storage-mgmt-local",
    "StorageBucket": "local-documents",
    "PubSubTopic": "document-events",
    "FirestoreDatabase": "(default)"
  },
  "Storage": {
    "PostgresDb": {
      "ConnectionString": "Host=localhost;Port=5432;Database=storagedb;Username=postgres;Password=localpassword;Include Error Detail=true",
      "EnableDetailedErrors": "true"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  }
}
```

**Environment Variables** (launchSettings.json):

```json
{
  "FIRESTORE_EMULATOR_HOST": "localhost:8080",
  "PUBSUB_EMULATOR_HOST": "localhost:8085",
  //"GOOGLE_APPLICATION_CREDENTIALS": "path/to/service-account.json" // No need if using google emulators
}
```

### Frontend (`app/webapp/`)

Create `.env.development`:

```env
VITE_API_BASE_URL=http://localhost:5001/api
VITE_ENVIRONMENT=development
```

### Cloud Functions

Create `app/gcf/storageazurefunctions/local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "FIRESTORE_EMULATOR_HOST": "localhost:8080",
    "PUBSUB_EMULATOR_HOST": "localhost:8085"
  }
}
```

Create `app/gcf/documentreceiverfunction/local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "FIRESTORE_EMULATOR_HOST": "localhost:8080",
    "PUBSUB_EMULATOR_HOST": "localhost:8085"
  }
}
```

## Build

### .NET Projects

```powershell
# Backend API
cd srcs/app/api
dotnet restore 
dotnet build

# Data Access Layer
cd ../dal/storagedal
dotnet restore
dotnet build

# Shared Entities
cd ../sharedentities/sharedentities
dotnet restore
dotnet build

# Cloud Functions (C#)
cd ../gcf/storageazurefunctions
dotnet restore
dotnet build
```

### Frontend Project

```powershell
cd srcs/app/webapp
npm install
npm run build
```

### Cloud Functions (Python)

```powershell
cd srcs/app/gcf/documentreceiverfunction
python -m venv .venv
.\.venv\Scripts\Activate.ps1  # Windows
# source .venv/bin/activate    # Linux/Mac
pip install -r requirements.txt
```

## Running Locally

### Option A: GCP Emulators (Firestore + Pub/Sub)

Start emulators in Docker:

```powershell
# Terminal 1: Start Firestore emulator
gcloud emulators firestore start --host-port=localhost:8080

# Terminal 2: Start Pub/Sub emulator
gcloud beta emulators pubsub start --host-port=localhost:8085
```

### Option B: Testcontainers (PostgreSQL)

Docker automatically starts PostgreSQL when needed:

```powershell
# Testcontainers.NET handles PostgreSQL startup via Docker
# No manual setup required; starts on first run
```

### Option C: Combined Setup (Recommended)

**Terminal 1 – Firestore Emulator:**

```powershell
gcloud emulators firestore start --host-port=localhost:8080
```

**Terminal 2 – Pub/Sub Emulator:**

```powershell
gcloud beta emulators pubsub start --host-port=localhost:8085
```

**Terminal 2 Use Podman compose:**

Used for (fake-gcs-server and PostgreSQL)

```powershell
 cd .\local-dev
 podman compose -f 'compose.yml' up -d
```

### Run projects

**Terminal 1 – Backend API:**

```powershell
cd srcs/app/api
dotnet run --launch-profile http-emulators
```

API: `http://localhost:5001`

**Terminal 2 – Frontend:**

```powershell
cd srcs/app/webapp
npm run dev
```

Frontend: `http://localhost:5173`

**Terminal 3 – Cloud Functions (optional):**

```powershell
cd srcs/app/gcf/storageazurefunctions
func start
```

## Verification

### API Health

```powershell
curl http://localhost:5001/health
```

### Test Endpoint

```powershell
curl http://localhost:5001/api/documenttype
```

### Frontend Endpoint

Open: `http://localhost:5173`

### Firestore Emulator UI

Access: `http://localhost:8080` (if running locally)

## Troubleshooting

### Issue: `FIRESTORE_EMULATOR_HOST` not set

**Solution**: Set in `appsettings.Development.json` or environment variables

```powershell
$env:FIRESTORE_EMULATOR_HOST="localhost:8080"
$env:PUBSUB_EMULATOR_HOST="localhost:8085"
```

### Issue: PostgreSQL connection timeout

**Solution**: Ensure Docker is running and Testcontainers has permission

```powershell
docker ps  # Verify Docker is working
```

### Issue: Frontend CORS error

**Solution**: Verify API CORS config allows `http://localhost:5173`

```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  }
}
```

### Issue: Firestore queries return empty

**Solution**: Ensure emulator is running and `FIRESTORE_EMULATOR_HOST` is set

```powershell
gcloud emulators firestore start --host-port=localhost:8080
```

## Production Build

### Backend

```powershell
cd srcs/app/api
dotnet publish -c Release -o ./publish
```

### Frontend

```powershell
cd srcs/app/webapp
npm run build
```

Output: `dist/` ready for Cloud Storage + Cloud CDN

### Deployment

See [Architecture Overview](architecture.md) for Cloud Run, Cloud Functions, and Terraform deployment details.

## Next Steps

- [Architecture Overview](architecture.md) – System design
- [Testing Guidelines](testing.md) – Test strategies
- [Project Coding Rules](../.github/custom-instructions.md) – Coding conventions
