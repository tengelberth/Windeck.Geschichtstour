using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Windeck.Geschichtstour.Backend.Pages
{
    /// <summary>
    /// Startseite der Anwendung. Leitet direkt auf den Adminbereich um.
    /// </summary>
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Umleitung auf die Admin-Übersicht
            return RedirectToPage("/Admin/Index");
        }
    }
}
