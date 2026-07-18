# 📦 Storage Management App – Document Upload Feature

Document management system built on **Google Cloud Platform**. Users upload documents through a web interface; the system stores files in Cloud Storage, metadata in Cloud SQL, and document types in Firestore.

## 🔗 Quick Links

| I want to... | Go to... |
|---|---|
| **Learn the architecture** | [Architecture Overview](docs/architecture.md) |
| **Set up & run locally** | [Build Instructions](docs/build.md) |
| **Understand testing** | [Testing Guidelines](docs/testing.md) |
| **See design decisions** | [Architecture Decisions](docs/adr/) |
| **Follow coding rules** | [Project Structure & Rules](.github/instructions/project-structure.instructions.md) |

## 🧰 Tech Stack

**Backend** → ASP.NET Core 10 | **Frontend** → Vue 3 + PrimeVue | **Infrastructure** → Terraform  
**Services** → Cloud Storage · Firestore · Cloud SQL (PostgreSQL) · Cloud Run · Pub/Sub

## 🗂 Project Structure

```
/
├── docs/                              # Documentation 
├── storage-management/
│   ├── app/                           # Application source code
│   │   ├── api/                       # ASP.NET Core API
│   │   ├── webapp/                    # Vue 3 frontend
│   │   ├── azf/                       # Cloud Functions
│   │   ├── dal/                       # Data access layer for Postgres database
│   │   └── sharedentities/            # Shared entities used for data acess layer
│   ├── diagram/                       # Architecture diagrams
```

## 🚀 Getting Started

2. **Want to code?** Follow [Build Instructions](docs/build.md) to set up locally

## 📚 Documentation

- [Build Instructions](docs/build.md) – Local development, dependencies, emulators
- [Deployment Guidelines](docs/deployment.md) – Deployment to GCP guidelines

## License

MIT License
