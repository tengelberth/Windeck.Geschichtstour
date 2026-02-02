using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;

namespace Windeck.Geschichtstour.Backend.Pages.Admin
{
    /// <summary>
    /// Liefert Kennzahlen und Schnellzugriffe fuer das Admin-Dashboard.
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

        public int StationCount { get; set; }
        public int TourCount { get; set; }
        public int CategoryCount { get; set; }
        public int MediaCount { get; set; }

        /// <summary>
        /// Laedt die fuer die Seite benoetigten Daten bei einer GET-Anfrage.
        /// </summary>
        public async Task OnGetAsync()
        {
            StationCount = await _dbContext.Stations.CountAsync();
            TourCount = await _dbContext.Tours.CountAsync();
            CategoryCount = await _dbContext.Categories.CountAsync();
            MediaCount = await _dbContext.MediaItems.CountAsync();
        }
    }
}


