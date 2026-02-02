# Windeck.Geschichtstour

Digitale Geschichtstour fuer die Gemeinde Windeck:
- `Backend/`: ASP.NET Core Backend (Admin + REST API)
- `Mobile/`: .NET MAUI App (Android/iOS)

## Projektziel

Das Projekt verbindet historische Inhalte vor Ort mit digitaler Nutzung:
- Stationen per QR-Code aufrufen
- Inhalte in der App lesen und ansehen
- Touren mit mehreren Stationen darstellen
- Inhalte zentral ueber die Admin-Oberflaeche pflegen

## Repository-Struktur

- `Backend/README.md`: Architektur, API, Admin, Hosting, Betrieb
- `Mobile/README.md`: App-Aufbau, Navigation, API-Nutzung, Build
- `Backend/wwwroot/uploads/`: hochgeladene Mediendateien (betrieblich relevant)

## Schnellstart

1. Voraussetzungen:
   - .NET 8 SDK
   - SQL Server (lokal oder remote)
   - optional: MAUI Workloads fuer Mobile
2. Backend starten:
   - `dotnet run --project Backend/Windeck.Geschichtstour.Backend.csproj`
3. Mobile starten:
   - `dotnet build Mobile/Windeck.Geschichtstour.Mobile.csproj`

## Dokumentationsstand

- C# Klassen und Methoden wurden mit XML-Kommentaren dokumentiert.
- Backend-Swagger nutzt die XML-Dokumentation.

## Mobile URL-Konfiguration

Die Mobile-App nutzt zentrale URL-Konfiguration statt Hardcoding:
- Datei: `Mobile/Resources/Raw/appsettings.json`
- Environment-Overrides:
  - `WINDECK_BACKEND_BASE_URL`
  - `WINDECK_PUBLIC_BASE_URL`
  - `WINDECK_ALLOWED_DEEPLINK_HOSTS` (kommagetrennt)

## Lizenz und Nutzung

Dieses Repository ist **nicht Open-Source im OSI-Sinn**.
Die Nutzung ist in `LICENSE.md` geregelt (proprietaere Source-Available-Lizenz).

Kurzfassung:
- Code-Urheberrecht: Tobias Engelberth
- Inhalte/Medien-Urheberrecht: Tourismus Windecker Laendchen e.V.
- Gemeinde Windeck: exklusives operatives Nutzungsrecht fuer produktiven Einsatz
- Dritte: Code nur fuer private, nicht-oeffentliche Projekte
- Bilder, Logos, PDFs und sonstige Medien: nur ansehen/herunterladen, keine Wiederverwendung

## Sicherheit vor Veroeffentlichung

Vor einer oeffentlichen Bereitstellung bitte insbesondere pruefen:
- `Backend/appsettings.json` (Admin-Zugangsdaten)
- `Backend/wwwroot/uploads/` (Dateiinhalte, Rechte, personenbezogene Daten)
- keine gesperrten Medieninhalte unerlaubt weitergeben oder weiterverwenden
- Checkliste: `OPEN_SOURCE_RELEASE_CHECKLIST.md`
