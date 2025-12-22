using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Models;

namespace Windeck.Geschichtstour.Backend.Pages.Admin.Stations
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        public IndexModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }


        public IList<Station> Stations { get; set; } = new List<Station>();

        public async Task OnGetAsync()
        {
            var query = _dbContext.Stations
                .Include(s => s.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.Trim();
                query = query.Where(s =>
                    s.Title.Contains(term) ||
                    s.Code.Contains(term) ||
                    (s.City != null && s.City.Contains(term)));
            }

            Stations = await query
                .OrderBy(s => s.Title)
                .ToListAsync();
        }


        /// <summary>
        /// Löscht eine Station anhand der ID.
        /// Wird über ein POST-Formular mit Handler "Delete" aufgerufen.
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var station = await _dbContext.Stations
                .Include(s => s.MediaItems)
                .Include(s => s.TourStops)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (station == null)
            {
                return NotFound();
            }

            // Zunächst abhängige TourStops löschen
            if (station.TourStops.Any())
            {
                _dbContext.TourStops.RemoveRange(station.TourStops);
            }

            // Medienobjekte löschen (Metadaten; Dateien im Storage musst du separat behandeln)
            if (station.MediaItems.Any())
            {
                _dbContext.MediaItems.RemoveRange(station.MediaItems);
            }

            _dbContext.Stations.Remove(station);
            TempData["SuccessMessage"] = "Station wurde gelöscht.";
            await _dbContext.SaveChangesAsync();

            // Nach dem Löschen wieder auf die Übersicht
            return RedirectToPage();
        }
    }
}
