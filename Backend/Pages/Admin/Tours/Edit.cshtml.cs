using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Models;

namespace Windeck.Geschichtstour.Backend.Pages.Admin.Tours
{
    /// <summary>
    /// Verarbeitet Erstellen und Bearbeiten von Datensaetzen in Formularseiten.
    /// </summary>
    public class EditModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Initialisiert eine neue Instanz von EditModel.
        /// </summary>
        public EditModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty]
        public Tour Tour { get; set; } = default!;

        public bool IsNew => Tour.Id == 0;

        /// <summary>
        /// Laedt die fuer die Seite benoetigten Daten bei einer GET-Anfrage.
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                Tour = new Tour();
            }
            else
            {
                Tour = await _dbContext.Tours
                    .Include(t => t.Stops)
                    .FirstOrDefaultAsync(t => t.Id == id.Value);

                if (Tour == null)
                    return NotFound();
            }

            return Page();
        }

        /// <summary>
        /// Verarbeitet das Absenden des Formulars und speichert Aenderungen.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (Tour.Id == 0)
            {
                _dbContext.Tours.Add(Tour);
                TempData["SuccessMessage"] = "Tour wurde erfolgreich angelegt.";
            }
            else
            {
                _dbContext.Attach(Tour).State = EntityState.Modified;
                TempData["SuccessMessage"] = "Tour wurde erfolgreich aktualisiert.";
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}


