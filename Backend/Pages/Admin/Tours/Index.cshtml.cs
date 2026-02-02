using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Models;

namespace Windeck.Geschichtstour.Backend.Pages.Admin.Tours
{
    /// <summary>
    /// Laedt und verwaltet die Tourenuebersicht im Adminbereich.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Initialisiert eine neue Instanz von IndexModel.
        /// </summary>
        public IndexModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IList<Tour> Tours { get; set; } = new List<Tour>();

        /// <summary>
        /// Laedt die fuer die Seite benoetigten Daten bei einer GET-Anfrage.
        /// </summary>
        public async Task OnGetAsync()
        {
            Tours = await _dbContext.Tours
                .Include(t => t.Stops)
                .OrderBy(t => t.Title)
                .ToListAsync();
        }

        /// <summary>
        /// Loescht den angeforderten Datensatz und aktualisiert die Uebersicht.
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var tour = await _dbContext.Tours
                .Include(t => t.Stops)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null)
                return NotFound();

            if (tour.Stops.Any())
            {
                _dbContext.TourStops.RemoveRange(tour.Stops);
            }

            _dbContext.Tours.Remove(tour);
            TempData["SuccessMessage"] = "Tour wurde gelöscht.";
            await _dbContext.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}


