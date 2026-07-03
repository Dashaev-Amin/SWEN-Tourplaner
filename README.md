# TourPlanner

Tour planning app - SWEN Projekt

**Repository:** https://github.com/Dashaev-Amin/SWEN-Tourplaner

## Setup

### Voraussetzungen
- .NET 8 SDK
- PostgreSQL
- Node.js (fuer Frontend)

### Environment-Variablen

| Variable | Beschreibung | Pflicht |
|----------|-------------|---------|
| `POSTGRES_PW` | Passwort fuer die PostgreSQL-Datenbank | Ja |

Setzen vor dem Start:
```bash
export POSTGRES_PW=deinpasswort        # Linux/Mac
set POSTGRES_PW=deinpasswort           # Windows CMD
$env:POSTGRES_PW="deinpasswort"        # PowerShell
```

### Datenbank einrichten

PostgreSQL installieren und eine DB erstellen:
```sql
CREATE DATABASE tourplanner;
```

Connection String ist in `TourPlanner.API/appsettings.json` konfiguriert. Das Passwort wird aus der ENV-Variable `POSTGRES_PW` gelesen.

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
