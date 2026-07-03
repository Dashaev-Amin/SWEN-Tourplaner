# Final-Abgabe: Status-Audit (Stand 2026-07-03)

## 1. Checklist

### MUST-HAVES

| # | Item | Status | Fundstelle | Notiz |
|---|------|--------|-----------|-------|
| 1 | Layer-Architektur (API/BL/DAL/Models) | ✅ fertig | `TourPlanner.API/`, `TourPlanner.BL/`, `TourPlanner.DAL/`, `TourPlanner.Models/` | 4 Projekte sauber getrennt. BL ist allerdings reiner Pass-through ohne echte Logik. |
| 2 | EF Core DbContext + Entities (Tour, TourLog) mit 1:n | ✅ fertig | `TourPlanner.DAL/TourPlannerDbContext.cs`, `TourPlanner.Models/Tour.cs`, `TourPlanner.Models/TourLog.cs` | Fluent-API-Config fuer FK + Cascade Delete. `Include(TourLogs)` in Repo. |
| 3 | Migrations vorhanden | ❌ fehlt | -- | Kein `Migrations/`-Ordner im Repo. README dokumentiert Befehle, aber keine Migrations committed. `Program.cs` hat Auto-Migrate auskommentiert. |
| 4 | Daten in Postgres persistiert (nicht In-Memory) | ✅ fertig | `appsettings.json` Connection-String, `Npgsql.EntityFrameworkCore.PostgreSQL` in DAL.csproj | Echte Postgres-Anbindung, kein In-Memory-Provider. |
| 5 | Mind. 1 Design Pattern | ✅ fertig | `TourPlanner.DAL/ITourRepository.cs`, `TourPlanner.BL/ITourService.cs`, `Program.cs` (DI) | Repository Pattern + Service Layer + Dependency Injection via Constructor. |
| 6 | Connection-String in Config (nicht im Code) | 🟡 teilweise | `TourPlanner.API/appsettings.json` | Wird via `IConfiguration.GetConnectionString()` geladen (gut). Aber `appsettings.json` enthaelt Klartext-Credentials (`postgres/postgres`) und ist NICHT in `.gitignore` -- also committed. |
| 7 | SQL-Injection-Schutz | ✅ fertig | `TourPlanner.DAL/TourRepository.cs`, `TourLogRepository.cs` | Ausschliesslich LINQ-to-Entities, kein rohes SQL. |
| 8 | ORS.org API (Directions) | ❌ fehlt | -- | Kein HttpClient, kein API-Call, kein ORS-Key irgendwo im Code. Komplett nicht implementiert. |
| 9 | Leaflet im Frontend (echte Map) | ❌ fehlt | `tourplanner-frontend/package.json`, `tour-detail.component.html` | Leaflet nicht in Dependencies. Map ist ein `<div class="map-placeholder">` mit Text "Karte wird spaeter hier angezeigt". Reiner Dummy. |
| 10 | Logging | ✅ fertig | `TourPlanner.BL/TourService.cs`, `TourLogService.cs`, `TourPlanner.API/Controllers/` | `ILogger<T>` injected in BL + Controller. Info-Logs auf Create/GetAll/Delete, Error-Logs in Catch-Bloecken. Default-Console-Sink. Kein log4net, aber Microsoft.Extensions.Logging reicht. |
| 11 | Unit Tests | ❌ fehlt | -- | Null Tests. Kein Test-Projekt, kein NUnit/xUnit/MSTest, keine .spec.ts-Dateien. `angular.json` hat `skipTests: true`. |

### FEATURES

| # | Item | Status | Fundstelle | Notiz |
|---|------|--------|-----------|-------|
| 12 | Tour CRUD (persistiert) | ✅ fertig | `TourController.cs`, `TourService.cs`, `TourRepository.cs`, Frontend `tour.service.ts` | Vollstaendiges REST-CRUD. Frontend ruft Backend-API via HttpClient. |
| 13 | TourLog CRUD (persistiert) | ✅ fertig | `TourLogController.cs`, `TourLogService.cs`, `TourLogRepository.cs`, Frontend `tour-log.service.ts` | Nested-Resource `api/tours/{tourId}/logs`. Prueft parent Tour existence. |
| 14 | Berechnete Attribute: Popularity | ❌ fehlt | -- | Kein computed Property, kein Endpoint, kein UI dafuer. |
| 15 | Berechnete Attribute: Child-Friendliness | ❌ fehlt | -- | Nicht implementiert. |
| 16 | Map-Image (echt, auf Filesystem) | ❌ fehlt | `Tour.cs` hat `RouteImage`-Property, `appsettings.json` hat `ImageDirectory: "Images"` | Property + Config existieren, aber kein Code der Bilder generiert, speichert oder served. Nur ein `<img>`-Tag im Frontend das `routeImage` anzeigen wuerde. |
| 17 | Volltextsuche (Tours + Logs + berechnete Attr.) | ❌ fehlt | -- | Kein Search-Endpoint, kein Suchfeld im Frontend, kein `Contains`/`Like` im DAL. |
| 18 | Import/Export | ❌ fehlt | -- | Kein Import/Export-Endpoint, kein UI dafuer. |
| 19 | Unique Feature | ❌ fehlt | -- | Kein erkennbares Unique Feature. Confirm-Dialog ist Standard-UI, kein Feature. |
| 20 | Input-Validierung Backend | 🟡 teilweise | `Tour.cs`, `TourLog.cs` | DataAnnotations (`[Required]`, `[Range]`, `[MaxLength]`). ASP.NET `[ApiController]` gibt automatisch 400 zurueck. Aber `TransportType` ist freier String ohne Enum-Constraint. Keine Validierung in BL-Layer. |
| 21 | Input-Validierung Frontend | ✅ fertig | `tour-form.component.ts`, `tour-log-form.component.ts` | Reactive Forms mit `Validators.required`, `.min()`, `.max()`. Error-Messages inline. `markAllAsTouched()` bei Submit. |
| 22 | Eigene Exceptions pro Layer | ❌ fehlt | -- | Keine Custom-Exceptions. Controller fangen generic `Exception` und returnen `BadRequest()` ohne Body. EF-Exceptions leaken potenziell nach oben. |

### PROTOKOLL-ARTEFAKTE

| # | Item | Status | Fundstelle | Notiz |
|---|------|--------|-----------|-------|
| 23 | Wireframes | ✅ fertig | `docs/exercise1-ui-wireframes/wireframe-variant-a.svg`, `wireframe-variant-b.svg` | 2 SVG-Varianten mit Annotations. |
| 24 | Klassendiagramm | ❌ fehlt | -- | Nicht gefunden. |
| 25 | Use-Case-Diagramm | ❌ fehlt | -- | Nicht gefunden. |
| 26 | Sequenzdiagramm (Volltextsuche) | ❌ fehlt | -- | Nicht gefunden (Volltextsuche selbst existiert auch nicht). |
| 27 | README mit Git-Link | 🟡 teilweise | `README.md` | README existiert mit Setup-Anleitung und Architektur-Beschreibung. Kein Git-Link enthalten. |
| 28 | Protokoll-PDF | ✅ fertig | `TourPlanner_Protokoll_Intermediate.pdf` | Intermediate-Protokoll vorhanden. Final-Protokoll noch nicht erstellt. |

---

## 2. Groesste Luecken fuer Final (priorisiert)

1. **Unit Tests (MUST-HAVE, 0-Punkte-Risiko)** -- Null Tests im gesamten Projekt. Braucht Test-Projekt + sinnvolle Tests fuer BL/DAL.
2. **ORS.org API-Anbindung (MUST-HAVE, 0-Punkte-Risiko)** -- Keine einzige Zeile Code dafuer. Braucht HttpClient, ORS-Key, Route-Abfrage, Distance/Time-Berechnung.
3. **Leaflet Map (MUST-HAVE, 0-Punkte-Risiko)** -- Nicht installiert, reiner Placeholder. Braucht npm-Paket + echte Map-Komponente mit Route-Anzeige.
4. **Migrations nicht committed** -- DB-Schema ist nicht reproduzierbar aus dem Repo.
5. **Berechnete Attribute** -- Popularity + Child-Friendliness komplett nicht implementiert.
6. **Volltextsuche** -- Kein Backend-Endpoint, kein Frontend-UI.
7. **Import/Export** -- Komplett fehlend.
8. **Map-Image-Generierung + Speicherung** -- Property existiert, Code fehlt.
9. **Diagramme (Klassen-, Use-Case, Sequenz)** -- Fuer Protokoll noetig, keines vorhanden.
10. **Unique Feature** -- Muss noch definiert und implementiert werden.
11. **Eigene Exceptions pro Layer** -- Nicht vorhanden.

---

## 3. Risiken (sieht fertig aus, ist aber Placeholder/Fake)

| Was | Risiko |
|-----|--------|
| **Map im Frontend** | `tour-detail.component.html` hat einen Map-Bereich -- ist aber ein `<div>` mit Platzhalter-Text, keine echte Karte. |
| **RouteImage-Property** | `Tour.cs` hat `RouteImage` + `appsettings.json` hat `ImageDirectory` -- suggeriert Bild-Speicherung, aber kein Code generiert, speichert oder served Bilder. |
| **BL-Layer** | Existiert strukturell, ist aber reiner Pass-through (jede Methode ruft nur das Repository auf). Keine Geschaeftslogik, keine Validierung, keine Berechnung. Bei der Abgabe koennte das als "leere Huelse" gewertet werden. |
| **Auto-Migration in Program.cs** | Auskommentierter Code suggeriert Feature, ist aber deaktiviert. Ohne committed Migrations kann die DB nicht aus dem Repo reproduziert werden. |
| **TransportType** | Freier String im Backend, aber Dropdown mit 4 festen Werten im Frontend. Backend akzeptiert jeden Wert -- keine serverseitige Validierung. |
| **Error-Handling** | Controller fangen `Exception` und returnen `BadRequest()` ohne Fehler-Details. EF-spezifische Exceptions (z.B. `DbUpdateException`) werden nicht differenziert behandelt und koennten impl-spezifische Details nach oben leaken. |
| **appsettings.json mit Credentials** | Connection-String mit `postgres/postgres` ist committed. Nicht in `.gitignore`. Kein Environment-Variable-Override. |
| **EF Core 8 auf .NET 9** | DAL.csproj targetted `net9.0` aber nutzt `Microsoft.EntityFrameworkCore 8.0.*`. Funktioniert, ist aber ein Version-Mismatch. |
