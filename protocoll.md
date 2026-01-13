# Media Ratings Platform – Entwicklungsprotokoll

## 1. Architektur und technische Entscheidungen
Die Anwendung wurde als eigenständiger REST-HTTP-Server mit `HttpListener` umgesetzt, wie in der Aufgabenstellung gefordert (kein ASP.NET).
Das Routing erfolgt zentral über einen `RequestRouter`, der Requests an spezialisierte Controller (User, Media, Rating, Favorites, Leaderboard) weiterleitet.

Zur Trennung von Business-Logik und Persistenz wurden Repository-Interfaces verwendet. Dadurch ist ein Austausch der Datenhaltung (In-Memory / PostgreSQL) ohne Änderungen an der Logik möglich.

## 2. Persistenz (PostgreSQL und In-Memory-Fallback)
Die Anwendung unterstützt zwei Persistenzvarianten:
- PostgreSQL (Zielarchitektur)
- In-Memory-Repositories (Fallback)

Im `RequestRouter` kann über einen Konfigurationsschalter (`usePostgres`) festgelegt werden, welche Variante verwendet wird.  
Bei gesetztem `usePostgres = true` greift die Anwendung auf PostgreSQL-Repositories zu.

Aufgrund technischer Probleme mit Docker auf dem Entwicklungsgerät konnte PostgreSQL lokal nicht stabil betrieben werden. Für die Abgabe wurde daher bewusst der In-Memory-Fallback verwendet, um alle Features stabil und vollständig demonstrieren zu können.

Die PostgreSQL-Anbindung (ConnectionFactory, SQL-Schema, Repository) ist vorbereitet und kann durch Umstellen der Konfiguration jederzeit aktiviert werden.

## 3. Authentifizierung und Sicherheit
Die Authentifizierung erfolgt tokenbasiert. Nach dem Login wird ein Token ausgegeben, das bei allen geschützten Endpunkten als Bearer-Token im HTTP-Header geprüft wird.

Zugriffsrechte werden serverseitig validiert (z.B. Bearbeiten/Löschen nur durch den Ersteller eines Mediums oder Ratings).

## 4. Unit Tests
Es wurden über 20 Unit Tests implementiert, die die zentrale Business-Logik abdecken (Token-Service, Media-, Rating- und Favorites-Repositories).
Getestet wurden insbesondere Rechteprüfungen, Mehrfachaktionen und Fehlerfälle.

## 5. Probleme und Lösungen
- Docker/PostgreSQL lokal nicht stabil lauffähig  
  → Lösung: Konfigurierbarer In-Memory-Fallback bei vorbereiteter PostgreSQL-Anbindung
- Routing ohne Framework  
  → Lösung: Manuelles Path-Parsing im zentralen Router

## 6. Zeitaufwand (geschätzt)
- HTTP-Server und Routing: ca. 10 Stunden  
- Authentifizierung und Security: ca. 5 Stunden  
- Media- und Rating-Logik: ca. 14 Stunden  
- Favorites und Leaderboard: ca. 5 Stunden  
- Unit Tests: ca. 7 Stunden  

Gesamtaufwand: ca. 41 Stunden




