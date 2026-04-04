# TourPlanner

Tour planning app - SWEN Projekt

## Setup

### Voraussetzungen
- .NET 8 SDK
- PostgreSQL

### Datenbank einrichten

PostgreSQL installieren und eine DB erstellen:
```sql
CREATE DATABASE tourplanner;
```

Connection String ist in `TourPlanner.API/appsettings.json` konfiguriert (default: localhost, postgres/postgres).

### Starten

```bash
cd TourPlanner.API
dotnet run
```

API laeuft dann auf `http://localhost:5000` (oder was auch immer in launchSettings steht).

Swagger UI: `http://localhost:5000/swagger`

### Projektstruktur

- **TourPlanner.API** - Web API, Controller, DI Setup
- **TourPlanner.BL** - Business Logic, Services
- **TourPlanner.DAL** - Data Access, EF Core, Repositories
- **TourPlanner.Models** - Geteilte Models (Tour, TourLog)

### EF Migrations

```bash
cd TourPlanner.API
dotnet ef migrations add InitialCreate --project ../TourPlanner.DAL
dotnet ef database update
```
