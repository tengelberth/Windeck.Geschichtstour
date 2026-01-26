using System.Threading.Tasks;
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
        public async Task<IActionResult> OnPostAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Account/Login");
        }

        public IActionResult OnGet()
        {
            // Logout nur per POST – GET leitet einfach zum Login weiter.
            return RedirectToPage("/Account/Login");
        }
    }
}
