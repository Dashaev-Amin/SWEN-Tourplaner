# TourPlanner

Tour Planner - SWEN2 Projekt (FH Technikum Wien)

**Repository:** https://github.com/Dashaev-Amin/SWEN-Tourplaner

**Protokoll:** siehe beiliegendes `TourPlanner_Protokoll_Final.pdf`

## Voraussetzungen

- .NET 8 SDK
- Node.js + npm (fuer Angular-Frontend)
- PostgreSQL

## Konfiguration

Erstelle `TourPlanner.API/appsettings.Development.json` (wird nicht committet):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=tourplanner;Username=postgres;Password=DEIN_PASSWORT"
  },
  "OrsApiKey": "DEIN_ORS_API_KEY"
}
```

- **ORS-API-Key:** kostenlos unter https://openrouteservice.org registrieren
- Alternativ per Environment-Variablen: `POSTGRES_CONNECTION_STRING`, `ORS_API_KEY`

## Datenbank einrichten

```sql
CREATE DATABASE tourplanner;
```

Die DB-Tabellen werden beim Start automatisch via EF-Migrations angelegt.

## Starten

```bash
# Backend (Port 5033)
cd TourPlanner.API
dotnet run

# Frontend (Port 4200, Proxy auf Backend)
cd tourplanner-frontend
npm install
npm start
```

- Backend-API: http://localhost:5033
- Swagger UI: http://localhost:5033/swagger
- Frontend: http://localhost:4200

## Projektstruktur

- **TourPlanner.API** - Web API, Controller, DI Setup
- **TourPlanner.BL** - Business Logic, Services, ORS-Integration, berechnete Attribute
- **TourPlanner.DAL** - Data Access, EF Core, Repositories, Migrations
- **TourPlanner.Models** - Geteilte Models (Tour, TourLog, TransportType)
- **TourPlanner.Tests** - 31 Unit Tests (NUnit + Moq)
- **tourplanner-frontend** - Angular 21 SPA (Leaflet-Karte, Volltextsuche, Import/Export)
- **docs/** - Wireframes, Status-Dokument

## Features

- Tour CRUD + TourLog CRUD (persistiert in PostgreSQL)
- ORS-Routing: Geocoding + Directions (Distanz, Zeit, Route automatisch berechnet)
- Leaflet-Karte mit Route-Anzeige
- Berechnete Attribute: Popularity + Child-Friendliness
- Volltextsuche ueber Tours, Logs und berechnete Labels
- Import/Export (JSON)
- GPX-Export (Unique Feature)
- Eigene Exceptions pro Layer
- Statisches Kartenbild (via staticmap.openstreetmap.de)
