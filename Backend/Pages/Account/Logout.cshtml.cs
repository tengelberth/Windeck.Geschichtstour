using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Windeck.Geschichtstour.Backend.Pages.Account
{
    /// <summary>
    /// Einfache Logout-Seite, die das Auth-Cookie löscht.
    /// </summary>
    public class LogoutModel : PageModel
    {
        /// <summary>
        /// Verarbeitet das Absenden des Formulars und speichert Aenderungen.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Account/Login");
        }

        /// <summary>
        /// Bereitet die Seite fuer eine GET-Anfrage vor.
        /// </summary>
        public IActionResult OnGet()
        {
            // Logout nur per POST – GET leitet einfach zum Login weiter.
            return RedirectToPage("/Account/Login");
        }
    }
}
