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
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Initialisiert eine neue Instanz von ManageMediaModel.
        /// </summary>
        public ManageMediaModel(AppDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
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
        /// Enthält die Dateigröße pro Medium (in bereits formatiertem Text).
        /// </summary>
        public Dictionary<int, string> MediaStorageById { get; } = new();

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

            BuildStorageInfo(MediaItems);

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

        /// <summary>
        /// Liefert den anzuzeigenden Speicherplatz für ein Medium.
        /// </summary>
        public string GetStorageDisplay(MediaItem media)
        {
            return MediaStorageById.TryGetValue(media.Id, out var value)
                ? value
                : "-";
        }

        private void BuildStorageInfo(IEnumerable<MediaItem> mediaItems)
        {
            MediaStorageById.Clear();

            foreach (var media in mediaItems)
            {
                MediaStorageById[media.Id] = ResolveStorageText(media.Url);
            }
        }

        private string ResolveStorageText(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return "-";
            }

            // Nur lokal gespeicherte Dateien können direkt ermittelt werden.
            if (!url.StartsWith('/'))
            {
                return "extern";
            }

            var relativePath = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var absolutePath = Path.Combine(_env.WebRootPath, relativePath);

            if (!System.IO.File.Exists(absolutePath))
            {
                return "nicht gefunden";
            }

            var fileSizeBytes = new FileInfo(absolutePath).Length;
            return FormatFileSize(fileSizeBytes);
        }

        private static string FormatFileSize(long bytes)
        {
            const double kb = 1024d;
            const double mb = kb * 1024d;

            if (bytes < kb)
            {
                return $"{bytes} B";
            }

            if (bytes < mb)
            {
                return $"ca. {Math.Round(bytes / kb):0} KB";
            }

            return $"ca. {(bytes / mb):0.0} MB";
        }
    }
}
