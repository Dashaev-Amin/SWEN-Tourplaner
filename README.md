# SWEN-MRP-Project
Project for University

## Prerequisites
- .NET SDK (project builds with `net8.0` and tests target `net9.0`)
- Docker Desktop (for PostgreSQL only)

## Database (PostgreSQL in Docker)
The schema is initialized automatically by Docker on first start.

```powershell
docker compose up -d
```

Connection settings (from `docker-compose.yml`):
- Host: `localhost`
- Port: `5432`
- Database: `mrp`
- User: `mrp`
- Password: `mrp`

If you need to reapply the schema manually:
`MRP/MRP/Persistence/schema.sql`

## Run the server locally (no container)
From the repo root:

```powershell
dotnet run --project MRP/MRP/MRP.csproj
```

## Run tests
From the repo root:

```powershell
dotnet test MRP/MRP.sln
```
