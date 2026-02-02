# Open-Source Release Checklist

Diese Checkliste hilft vor einer oeffentlichen Veroeffentlichung des Repositories.

## 1) Secrets und Konfiguration

- [x] `Backend/appsettings.json` enthaelt keine produktiven Zugangsdaten.
- [ ] Admin-Zugangsdaten werden ueber Environment Variables/Secret Store gesetzt.
- [ ] Keine API-Keys, Tokens oder Passwoerter in Commits/History.

## 2) Dateien und Inhalte

- [ ] `Backend/wwwroot/uploads/` enthaelt keine personenbezogenen oder internen Daten.
- [x] Upload-Verzeichnis bleibt ungetrackt (`.gitignore` + `.gitkeep`).
- [x] Bilder, Logos, PDFs und Medien sind als nicht wiederverwendbar gekennzeichnet.
- [x] Falls Dateien nur lesbar/herunterladbar sein sollen, ist das in README und Lizenz klar benannt.

## 3) Lizenz und Recht

- [x] `LICENSE.md` ist im Repo-Root vorhanden.
- [x] README verweist auf proprietaere Source-Available-Lizenz.
- [x] Code-Urheberrecht ist Tobias Engelberth zugeordnet.
- [x] Inhalts-/Medienrechte sind Tourismus Windecker Laendchen e.V. zugeordnet.
- [ ] Marken-/Namensrechte sind korrekt gekennzeichnet.

## 4) Technische Qualitaet

- [x] Backend baut erfolgreich: `dotnet build Backend/Windeck.Geschichtstour.Backend.csproj`
- [x] Mobile baut auf Zielplattform(en) (Android-CLI-Build erfolgreich).
- [ ] Wichtige Kernflows wurden manuell getestet (Login, API, Deeplinks, Medien).

## 5) Domain und Deployment

- [x] `Mobile/Resources/Raw/appsettings.json` enthaelt die gewuenschte Backend-/Public-URL.
- [ ] Optional: Environment-Variablen gesetzt (`WINDECK_BACKEND_BASE_URL`, `WINDECK_PUBLIC_BASE_URL`, `WINDECK_ALLOWED_DEEPLINK_HOSTS`).
- [x] Deeplink-Hosts in Android/iOS Manifesten sind korrekt fuer Ziel-Domain(s).
