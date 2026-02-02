using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Models;

namespace Windeck.Geschichtstour.Backend.Pages.Admin.Stations
{
    /// <summary>
    /// Razor Page zum Anlegen und Bearbeiten einer Station.
    /// Wird unter /Admin/Stations/Edit bzw. /Admin/Stations/Edit/{id} aufgerufen.
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

        /// <summary>
        /// Station, die im Formular bearbeitet wird.
        /// [BindProperty] sorgt dafür, dass Formulareingaben automatisch
        /// in diese Eigenschaft gemappt werden.
        /// </summary>
        [BindProperty]
        public Station Station { get; set; } = default!;

        /// <summary>
        /// Liste der Kategorien für das Dropdown im Formular.
        /// </summary>
        public List<SelectListItem> CategoryOptions { get; set; } = new();

        /// <summary>
        /// Gibt an, ob es sich um eine neue Station (true) oder eine bestehende (false) handelt.
        /// </summary>
        public bool IsNew => Station.Id == 0;

        /// <summary>
        /// Lädt eine bestehende Station oder bereitet das Formular für eine neue Station vor.
        /// </summary>
        /// <param name="id">Optional: ID der zu bearbeitenden Station.</param>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Kategorien für das Dropdown vorbereiten
            await LoadCategoriesAsync();

            if (id == null)
            {
                // Neue Station anlegen
                Station = new Station();
            }
            else
            {
                // Bestehende Station aus der DB laden
                Station = await _dbContext.Stations
                    .Include(s => s.Category)
                    .FirstOrDefaultAsync(s => s.Id == id.Value);

                if (Station == null)
                {
                    return NotFound();
                }
            }

            return Page();
        }

        /// <summary>
        /// Verarbeitet das Formular beim Absenden (POST).
        /// Legt eine neue Station an oder aktualisiert eine bestehende.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // Kategorien wieder laden, falls Validierungsfehler auftreten
            await LoadCategoriesAsync();

            // Prüfen, ob der Code bereits von einer anderen Station verwendet wird
            var codeExists = await _dbContext.Stations
                .AnyAsync(s => s.Code == Station.Code && s.Id != Station.Id);

            if (codeExists)
            {
                ModelState.AddModelError("Station.Code",
                    "Der eingegebene Code wird bereits von einer anderen Station verwendet.");

                return Page();
            }

            if (!ModelState.IsValid)
            {
                // Formular mit Fehlermeldungen erneut anzeigen
                return Page();
            }

            if (Station.Id == 0)
            {
                // Neue Station
                _dbContext.Stations.Add(Station);
                TempData["SuccessMessage"] = "Station wurde erfolgreich angelegt.";
            }
            else
            {
                // Bestehende Station aktualisieren
                _dbContext.Attach(Station).State = EntityState.Modified;
                TempData["SuccessMessage"] = "Station wurde erfolgreich aktualisiert.";
            }

            await _dbContext.SaveChangesAsync();

            // Nach dem Speichern zurück zur Übersichtsseite
            return RedirectToPage("Index");
        }

        /// <summary>
        /// Lädt alle Kategorien aus der Datenbank und befüllt das Dropdown.
        /// </summary>
        private async Task LoadCategoriesAsync()
        {
            CategoryOptions = await _dbContext.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            // Option "keine Kategorie" ergänzen
            CategoryOptions.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "(keine Kategorie)"
            });
        }
    }
}
