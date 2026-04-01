using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Models;
using Windeck.Geschichtstour.Backend.Services;

namespace Windeck.Geschichtstour.Backend.Pages.Admin.Stations
{
    /// <summary>
    /// Lädt und verwaltet die Stationsübersicht im Adminbereich.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Initialisiert eine neue Instanz von IndexModel.
        /// </summary>
        public IndexModel(AppDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public IList<Station> Stations { get; set; } = new List<Station>();

        /// <summary>
        /// Lädt die für die Seite benötigten Daten bei einer GET-Anfrage.
        /// </summary>
        public async Task OnGetAsync()
        {
            IQueryable<Station> query = _dbContext.Stations
                .Include(s => s.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                string term = SearchTerm.Trim();
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
            Station? station = await _dbContext.Stations
                .Include(s => s.MediaItems)
                .Include(s => s.TourStops)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (station == null)
            {
                return NotFound();
            }

            // Zunächst abhängige TourStops löschen.
            if (station.TourStops.Any())
            {
                _dbContext.TourStops.RemoveRange(station.TourStops);
            }

            if (station.MediaItems.Any())
            {
                _dbContext.MediaItems.RemoveRange(station.MediaItems);
            }

            _dbContext.Stations.Remove(station);
            await _dbContext.SaveChangesAsync();

            bool storageCleanupSucceeded = StationUploadStorage.TryDeleteStationUploadDirectory(_env.WebRootPath, station.Id);

            TempData["SuccessMessage"] = storageCleanupSucceeded
                ? "Station wurde gelöscht."
                : "Station wurde gelöscht. Die zugehörigen Upload-Dateien konnten auf dem Server nicht vollständig entfernt werden.";

            return RedirectToPage();
        }
    }
}
