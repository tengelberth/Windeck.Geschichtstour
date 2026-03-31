# Open-Source Release Checklist

Diese Checkliste hilft vor einer öffentlichen Veröffentlichung des Repositories.

## 1) Secrets und Konfiguration

- [ ] `Backend/appsettings.json` enthält keine produktiven Zugangsdaten.
- [ ] Admin-Zugangsdaten werden über Environment Variables/Secret Store gesetzt.
- [ ] Keine API-Keys, Tokens oder Passwörter in Commits/History.

## 2) Dateien und Inhalte

- [ ] `Backend/wwwroot/uploads/` enthält keine personenbezogenen oder internen Daten.
- [ ] Upload-Verzeichnis bleibt ungetrackt (`.gitignore` + `.gitkeep`).
- [ ] Bilder, Logos, PDFs und Medien sind als nicht wiederverwendbar gekennzeichnet.
- [ ] Falls Dateien nur lesbar/herunterladbar sein sollen, ist das in README und Lizenz klar benannt.

## 3) Lizenz und Recht

- [ ] `LICENSE.md` ist im Repo-Root vorhanden.
- [ ] README verweist auf proprietäre Source-Available-Lizenz.
- [ ] Code-Urheberrecht ist Tobias Engelberth zugeordnet.
- [ ] Inhalts-/Medienrechte sind Tourismus Windecker Ländchen e.V. zugeordnet.
- [ ] Marken-/Namensrechte sind korrekt gekennzeichnet.

## 4) Technische Qualität

- [ ] Backend baut erfolgreich: `dotnet build Backend/Windeck.Geschichtstour.Backend.csproj`
- [ ] Mobile baut auf Zielplattform(en) (Android-CLI-Build erfolgreich).
- [ ] Wichtige Kernflows wurden manuell getestet (Login, API, Deeplinks, Medien).

## 5) Domain und Deployment

- [ ] `Mobile/Resources/Raw/appsettings.json` enthält die gewünschte Backend-/Public-URL.
- [ ] Optional: Environment-Variablen gesetzt (`WINDECK_BACKEND_BASE_URL`, `WINDECK_PUBLIC_BASE_URL`, `WINDECK_ALLOWED_DEEPLINK_HOSTS`).
- [ ] Deeplink-Hosts in Android/iOS Manifesten sind korrekt für Ziel-Domain(s).
