# Digitale Geschichtstour Windeck – Backend

Dieses Repository enthält das Backend für das Projekt **„Digitale Geschichtstour Windeck“**.  
Ziel des Projekts ist es, historische Inhalte der Gemeinde Windeck über QR-Codes vor Ort und eine mobile App zugänglich zu machen.  
Die Pflege der Inhalte erfolgt ausschließlich über eine geschützte Administrationsoberfläche.

Das Backend besteht aus:

- einer **ASP.NET Core 8** Anwendung mit
  - **Razor Pages** für die Admin-Oberfläche und
  - einer **Web-API** (JSON) für die mobile App,
- einer **SQL-Server-Datenbank** (via Entity Framework Core).

Die mobile App wird mit **.NET MAUI** umgesetzt und ist in einem separaten Projekt vorgesehen.

---

## 1. Technologiestack

- .NET 8 (ASP.NET Core)
- Razor Pages (Admin-Oberfläche)
- ASP.NET Core Web API (Controller-basiert)
- Entity Framework Core (Code-First, SQL Server)
- Bootstrap 5 & Bootstrap Icons (UI/Styling)
- Quill (Rich-Text-Editor für Langbeschreibungen)
- Swagger / Swashbuckle (API-Dokumentation)
- .NET MAUI (separate mobile App, nicht in diesem Repo)

---

## 2. Architekturüberblick

Die Lösung folgt einer klar getrennten Architektur:

- **Backend**
  - Admin-Frontend mit Razor Pages
  - REST-ähnliche Web-API für Stationen, Touren etc.
  - Persistenzschicht mit EF Core und SQL Server
- **Client**
  - .NET MAUI App (Android/iOS), die ausschließlich die Web-API konsumiert
- **Kein direkter DB-Zugriff** aus der App:  
  alle Zugriffe laufen über die API → bessere Sicherheit, Wartbarkeit und klare Verantwortlichkeiten.

---

## 3. Projektstruktur (Backend)

Grobe Struktur des Projekts:

- `Data/`
  - `AppDbContext` – EF-Core-DbContext mit  
    `DbSet<Station>`, `DbSet<Category>`, `DbSet<Tour>`, `DbSet<TourStop>`, `DbSet<MediaItem>`
  - `Migrations/` – EF-Code-First-Migrationen
- `Models/`
  - `Station`, `Category`, `Tour`, `TourStop`, `MediaItem`  
    (inkl. DataAnnotations für Validierung)
- `Controllers/`
  - `StationsController` – GET-Endpunkte für Stationsdaten (inkl. by-code)
  - `ToursController` – GET-Endpunkte für Tourdaten
- `Pages/Admin/`
  - `Index` – Dashboard (Kennzahlen + Einstieg)
  - `Stations/` – `Index`, `Edit`, `ManageMedia` etc.
  - `Categories/` – `Index`, `Edit`
  - `Tours/` – `Index`, `Edit`, `ManageStops`
  - optional: `Help` – Anwenderdoku im System
- `Pages/Account/`
  - `Login`, `Logout` – Login/Logout für Admin
- `Pages/Shared/`
  - `_AdminLayout` – zentrales Layout (Sidebar, Branding, Alerts)
- `wwwroot/`
  - `css/site.css` – Projekt-spezifische Styles (Farben, Layout, Buttons)
  - `images/` – Logo, Headerbilder, Favicon
  - `uploads/` – von Nutzern hochgeladene Medien (z. B. `/uploads/stations/{StationId}/...`)

---

## 4. Datenmodell (Domäne)

Zentrale Domänenklassen:

- **Category**
  - `Id`, `Name`, `Description`
  - Navigation: `Stations : List<Station>`
- **Station**
  - `Id`, `Code` (eindeutig), `Title`
  - `ShortDescription`, `LongDescription`
  - Adresse: `Street`, `HouseNumber`, `ZipCode`, `City` (optional)
  - Koordinaten: `Latitude`, `Longitude` (optional)
  - `CategoryId` (nullable), Navigation `Category`
  - Navigation: `MediaItems : List<MediaItem>`, `TourStops : List<TourStop>`
- **Tour**
  - `Id`, `Title`, `Description`
  - Navigation: `Stops : List<TourStop>`
- **TourStop**
  - `Id`, `TourId`, `StationId`, `Order`
  - Navigation: `Tour`, `Station`
- **MediaItem**
  - `Id`, `StationId`
  - `MediaType` (z. B. „Image“, „Audio“, „Video“)
  - `Url`
  - `Caption` (optional)
  - `SortOrder`
  - Navigation: `Station`

Beziehungen:

- Category `1 .. *` Station
- Station `1 .. *` MediaItem
- Tour `1 .. *` TourStop
- Station `1 .. *` TourStop

---

## 5. Konfiguration & Setup (Entwicklungsumgebung)

1. **Voraussetzungen**
   - .NET 8 SDK
   - SQL Server (lokal oder remote)

2. **Konfiguration**
   - In `appsettings.json` den Connection String unter `"DefaultConnection"` anpassen, z. B.:

     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=.;Database=GeschichtstourDb;Trusted_Connection=True;TrustServerCertificate=True"
     }
     ```

3. **Datenbank-Migrationen**
   - Paket-Manager-Konsole oder `dotnet ef` verwenden:

     ```bash
     # Bei Bedarf
     dotnet ef migrations add InitialCreate
     dotnet ef database update
     ```

4. **Starten**
   - In Visual Studio: Startprojekt ausführen (F5)
   - oder:

     ```bash
     dotnet run
     ```

5. **Aufruf**
   - Adminbereich: `https://localhost:<port>/Admin`
   - API: `https://localhost:<port>/api/stations`, `.../api/tours`
   - Swagger: `https://localhost:<port>/swagger`

---

## 6. Authentifizierung & Sicherheit

### Adminbereich

- Cookie-basierte Authentifizierung via `AddAuthentication().AddCookie(...)`
- `options.Conventions.AuthorizeFolder("/Admin")` schützt alle Admin-Seiten
- Login-Seite `/Account/Login` ist explizit von Auth-Pflicht ausgenommen
- Zugangsdaten (Benutzername/Passwort) werden in `appsettings.json` konfiguriert und beim Login geprüft
- Nach erfolgreichem Login wird ein Auth-Cookie gesetzt, Logout entfernt es wieder

> Hinweis: Für produktive Nutzung sollten Passwörter nicht im Klartext stehen, sondern gehasht und idealerweise über einen Secret Store (z. B. Azure Key Vault) verwaltet werden.

### Öffentliche API

- Aktuell **nur lesende Endpunkte** (GET) für Stationen und Touren
- Keine öffentlichen Create/Update/Delete-Endpunkte
- Schreiboperationen ausschließlich über den geschützten Adminbereich

Für zukünftige Erweiterungen (weitere Clients mit Schreibrechten) bietet sich eine Erweiterung um JWT-basierte Authentifizierung für die API an.

---

## 7. Medien-Upload & Wiederverwendung

- Medien werden über den Adminbereich pro Station verwaltet
- Optionen:
  - URL zu einem bereits vorhandenen Medium setzen
  - Datei direkt hochladen
- Hochgeladene Dateien werden unterhalb von `wwwroot/uploads/` (z. B. `/uploads/stations/{StationId}/...`) gespeichert
- Die relative URL wird in `MediaItem.Url` hinterlegt

**Wichtig:**  
Wenn ein Bild/Medium mehrfach verwendet werden soll, sollte es **nicht mehrfach hochgeladen** werden.  
Stattdessen:

1. Medienverwaltung der Station öffnen, wo das Medium bereits existiert
2. URL in der Tabelle kopieren (z. B. `/uploads/stations/3/bild123.png`)
3. In einer anderen Station ein neues MediaItem anlegen und diese URL in das URL-Feld einfügen

Das spart Speicherplatz und hält die Medienstruktur sauber.

---

## 8. API-Dokumentation (Swagger)

- Swagger ist im Projekt über **Swashbuckle** integriert
- Aufruf (Standard): `https://localhost:<port>/swagger`
- Bietet:
  - Überblick über alle Endpunkte
  - Ausprobieren der API direkt im Browser
  - Generierte OpenAPI-Spezifikation (z. B. für Client-Generatoren)

Je nach Umgebung kann Swagger in `Program.cs` nur für Development oder auch für Produktion aktiviert werden.

---

## 9. Ausblick

Auf Basis dieses Backends kann die .NET-MAUI-App:

- Stationen (inkl. Medien und Koordinaten) anzeigen,
- Touren mit sortierten Stops darstellen,
- die Navigations- und Kartenfunktionen des Geräts nutzen (z. B. Google Maps mit übergebenen Koordinaten).

Mögliche Erweiterungen:

- Rollen- und Rechtekonzept (mehrere Benutzer, unterschiedliche Berechtigungen)
- Mehrsprachigkeit (DE/EN)
- Erweiterte Auswertungen / Analytics (z. B. meistgenutzte Stationen)
- Auslagerung von Medien in Cloud-Storage (z. B. Azure Blob Storage)
- Erweiterte API (z. B. Feedback, Favoriten, Nutzeraktionen)
