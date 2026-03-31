# Mobile - Windeck.Geschichtstour.Mobile

.NET MAUI App für die digitale Geschichtstour Windeck.

Die App konsumiert ausschliesslich das Backend unter `Backend/` und bietet:
- Startseite mit Code-Eingabe
- QR-Scanner für Stationscodes
- Stationslisten- und Kartenansicht
- Tourenliste und Tour-Teaser
- Medienanzeige je Station

## Architektur

- Views: XAML-Seiten in `Views/`
- ViewModels: Logik und Commands in `ViewModels/`
- Services: API-Zugriff in `Services/`
- Models: DTOs für API-Daten in `Models/`
- Helper/Behaviors/Controls: UI- und Plattform-nahe Hilfen

## API-Anbindung

- Zentrale API-Klasse: `Services/APIClient.cs`
- JSON Source Generator: `Services/ApiJsonContext.cs`
- Zentrale URL-Konfiguration: `Resources/Raw/appsettings.json`
- Optionale Environment-Variablen:
  - `WINDECK_BACKEND_BASE_URL`
  - `WINDECK_PUBLIC_BASE_URL`
  - `WINDECK_ALLOWED_DEEPLINK_HOSTS` (kommagetrennt)

## Build und Start

Voraussetzungen:
- .NET 10 SDK
- MAUI Workloads
- Android SDK bzw. iOS Toolchain (je nach Zielplattform)

Beispiele:
- Build: `dotnet build Mobile/Windeck.Geschichtstour.Mobile.csproj`
- Android Start (CLI): `dotnet build -t:Run -f net8.0-android Mobile/Windeck.Geschichtstour.Mobile.csproj`

## Deep Links

Die App verarbeitet Deeplinks für Stationen und Touren:
- `/station?code=...`
- `/tour?id=...`

Hinweis: Android-/iOS-Intentfilter bzw. Associated Domains sind plattformspezifisch
und weiterhin statisch in den Plattformdateien hinterlegt.
Bei Domainwechsel müssen daher auch `Mobile/Platforms/Android/MainActivity.cs`
und `Mobile/Platforms/iOS/Entitlements.plist` angepasst werden.

## Open-Source-Hinweis

Auch für das Mobile-Projekt gilt die proprietäre Lizenz aus `LICENSE.md` (Repository-Root).
Wichtige Kurzfassung:
- Code-Urheberrecht: Tobias Engelberth
- Inhalte/Medien-Urheberrecht: Tourismus Windecker Ländchen e.V.
- Medienassets (Bilder, Logos, PDFs usw.) dürfen nicht wiederverwendet werden.
