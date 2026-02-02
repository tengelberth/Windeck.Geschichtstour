using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Windeck.Geschichtstour.Backend.Pages.Account
{
    /// <summary>
    /// Einfache Login-Seite für den Adminbereich.
    /// Prüft die Zugangsdaten gegen die Konfiguration (appsettings.json)
    /// und legt bei Erfolg ein Auth-Cookie an.
    /// </summary>
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initialisiert eine neue Instanz von LoginModel.
        /// </summary>
        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// ViewModel für die Login-Eingaben (Benutzername/Passwort).
        /// </summary>
        public class LoginInputModel
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new LoginInputModel();

        /// <summary>
        /// Optional: ReturnUrl, damit man nach dem Login wieder dahin kommt,
        /// wo man ursprünglich hin wollte (z. B. /Admin/Stations).
        /// </summary>
        [FromQuery]
        public string? ReturnUrl { get; set; }

        /// <summary>
        /// Bereitet die Seite fuer eine GET-Anfrage vor.
        /// </summary>
        public void OnGet()
        {
            // Wird einfach nur angezeigt. ReturnUrl kann aus der Query kommen.
        }

        /// <summary>
        /// Verarbeitet das Absenden des Formulars und speichert Aenderungen.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Admin-Daten aus der Konfiguration lesen
            var adminUser = _configuration["AdminAuth:Username"];
            var adminPassword = _configuration["AdminAuth:Password"];

            // Einfache Prüfung: Benutzername + Passwort müssen exakt übereinstimmen.
            if (Input.Username == adminUser && Input.Password == adminPassword)
            {
                // Claims für den angemeldeten Benutzer
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, Input.Username),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Auth-Cookie erstellen
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                // Nach erfolgreichem Login zur ursprünglichen Seite oder Admin-Startseite
                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }

                return RedirectToPage("/Admin/Index");
            }

            // Falls falsche Daten: Fehlermeldung anzeigen
            ModelState.AddModelError(string.Empty, "Benutzername oder Passwort ist ungültig.");
            return Page();
        }
    }
}
