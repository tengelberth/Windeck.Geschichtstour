# Backend - Windeck.Geschichtstour.Backend

ASP.NET Core 10 Backend für die digitale Geschichtstour der Gemeinde Windeck.

Das Backend besteht aus:
- Admin-Oberfläche (Razor Pages) zur Inhaltsverwaltung
- REST API (JSON) für die mobile App
- Entity Framework Core mit SQL Server

## Verantwortungsbereich

- Verwaltung von Kategorien, Stationen, Medien und Touren
- Login-geschützter Admin-Bereich unter `/Admin`
- Öffentliche, lesende API-Endpunkte unter `/api/...`

## Kerntechnologien

- .NET 10 / ASP.NET Core
- Razor Pages
- ASP.NET Core Web API
- Entity Framework Core + SQL Server
- Swashbuckle / Swagger

## Projektstruktur

- `Controllers/`: API-Endpunkte (`StationsController`, `ToursController`)
- `Data/`: `AppDbContext`, `SeedData`
- `Dtos/`: Transportobjekte für API-Antworten
- `Models/`: Domainenmodell
- `Pages/Admin/`: Admin-UI
- `Pages/Account/`: Login/Logout
- `wwwroot/uploads/`: hochgeladene Mediendateien

## Lokales Setup

1. Voraussetzungen
   - .NET 10 SDK
   - SQL Server
2. Konfiguration
   - `appsettings.json` prüfen (`ConnectionStrings:DefaultConnection`)
3. Start
   - `dotnet run --project Backend/Windeck.Geschichtstour.Backend.csproj`
4. Zugriff
   - Admin: `https://localhost:<port>/Admin`
   - Swagger: `https://localhost:<port>/swagger`

## API-überblick

- `GET /api/stations`
- `GET /api/stations/by-code/{code}`
- `GET /api/tours`
- `GET /api/tours/{id}`

## Sicherheitshinweise

- In `appsettings.json` liegen aktuell Admin-Zugangsdaten.
- Vor externer Veröffentlichung Zugangsdaten immer durch sichere Werte ersetzen.
- Für Produktion Secrets nicht im Repo speichern (z. B. User Secrets / Key Vault).

## Open-Source-Hinweis

Dieses Projekt ist **quelloffen einsehbar**, aber **proprietär lizenziert**.
Details stehen in der Datei `LICENSE.md` im Repository-Root.
Wichtige Kurzfassung:
- Code-Urheberrecht: Tobias Engelberth
- Inhalte/Medien-Urheberrecht: Tourismus Windecker Ländchen e.V.
- Medienassets (Bilder, Logos, PDFs usw.) dürfen nicht wiederverwendet werden.
