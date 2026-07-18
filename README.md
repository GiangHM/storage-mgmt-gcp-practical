# 📦 Storage Management App – Document Upload Feature

Document management system built on **Google Cloud Platform**. Users upload documents through a web interface; the system stores files in Cloud Storage, metadata in Cloud SQL, and document types in Firestore.

## 🔗 Quick Links

| I want to... | Go to... |
|---|---|
| **Set up & run locally** | [Build Instructions](docs/build.md) |
| **Deployment Guidelines** | [Deployment Guidelines](docs/deployment.md) |

## 🧰 Tech Stack

**Backend** → ASP.NET Core 10 | **Frontend** → Vue 3 + PrimeVue | **Infrastructure** → Terraform  
**Google Cloud Services** → Cloud Storage · Firestore · Cloud SQL (PostgreSQL) · Cloud Run · Pub/Sub

## 🗂 Project Structure

```
/
├── docs/                              # Documentation 
├── storage-management/
│   ├── app/                           # Application source code
│   │   ├── api/                       # ASP.NET Core API
│   │   ├── webapp/                    # Vue 3 frontend
│   │   ├── dal/                       # Data access layer for Postgres database
│   │   └── sharedentities/            # Shared entities used for data acess layer
```

## 🚀 Getting Started

2. **Want to code?** Follow [Build Instructions](docs/build.md) to set up locally

## Result

- Storage API
  <img width="1588" height="863" alt="image" src="https://github.com/user-attachments/assets/e4dc995f-f9e6-432e-90f9-830a285e4afc" />

- Vue App
   <img width="1887" height="493" alt="image" src="https://github.com/user-attachments/assets/5550c0ad-8d21-46cf-9592-00a1c4ccfc8e" />

   <img width="1890" height="836" alt="Recording 2026-07-18 232227" src="https://github.com/user-attachments/assets/519338bb-1eba-4e81-98e8-d74880c54ad8" />



## 📚 Documentation

- [Build Instructions](docs/build.md) – Local development, dependencies, emulators
- [Deployment Guidelines](docs/deployment.md) – Deployment to GCP guidelines

## License

MIT License
