using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Models;

namespace Windeck.Geschichtstour.Backend.Pages.Admin.Stations
{
    /// <summary>
    /// Verwaltung der Medien (Bilder, Audio, Video) zu einer Station.
    /// </summary>
    public class ManageMediaModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Initialisiert eine neue Instanz von ManageMediaModel.
        /// </summary>
        public ManageMediaModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Station, zu der die Medien gehören.
        /// </summary>
        public Station Station { get; set; } = default!;

        /// <summary>
        /// Alle Medien der Station, sortiert nach SortOrder.
        /// </summary>
        public List<MediaItem> MediaItems { get; set; } = new();

        /// <summary>
        /// Laedt die fuer die Seite benoetigten Daten bei einer GET-Anfrage.
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int stationId)
        {
            Station = await _dbContext.Stations
                .Include(s => s.MediaItems)
                .FirstOrDefaultAsync(s => s.Id == stationId);

            if (Station == null)
            {
                return NotFound();
            }

            MediaItems = Station.MediaItems
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.Id)
                .ToList();

            return Page();
        }

        /// <summary>
        /// Löscht ein Medium anhand seiner ID.
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync(int stationId, int mediaId)
        {
            var media = await _dbContext.MediaItems
                .FirstOrDefaultAsync(m => m.Id == mediaId && m.StationId == stationId);

            if (media == null)
            {
                return NotFound();
            }

            _dbContext.MediaItems.Remove(media);
            TempData["SuccessMessage"] = "Medium wurde gelöscht.";
            await _dbContext.SaveChangesAsync();

            return RedirectToPage(new { stationId });
        }
    }
}
