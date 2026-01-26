using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Models;

namespace Windeck.Geschichtstour.Backend.Pages.Admin.Stations
{
    /// <summary>
    /// Anlegen und Bearbeiten eines MediaItems zu einer Station.
    /// </summary>
    public class EditMediaModel : PageModel
    {
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        public EditMediaModel(AppDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
        }


        /// <summary>
        /// Station, zu der das Medium gehört.
        /// </summary>
        public Station Station { get; set; } = default!;

        /// <summary>
        /// Das zu bearbeitende Medium.
        /// </summary>
        [BindProperty]
        public MediaItem Media { get; set; } = default!;

        /// <summary>
        /// Optional hochzuladende Datei. Wenn gesetzt, wird sie auf dem Server gespeichert
        /// und die Url-Eigenschaft des MediaItems automatisch gesetzt.
        /// </summary>
        [BindProperty]
        public IFormFile? UploadFile { get; set; }


        /// <summary>
        /// Gibt an, ob es sich um ein neues Medium handelt.
        /// </summary>
        public bool IsNew => Media.Id == 0;

        public async Task<IActionResult> OnGetAsync(int stationId, int? mediaId)
        {
            // Station laden
            Station = await _dbContext.Stations.FirstOrDefaultAsync(s => s.Id == stationId);
            if (Station == null)
            {
                return NotFound();
            }

            if (mediaId == null)
            {
                // Neues Medium
                Media = new MediaItem
                {
                    StationId = stationId,
                    MediaType = "Image",
                    SortOrder = 1
                };
            }
            else
            {
                Media = await _dbContext.MediaItems
                    .FirstOrDefaultAsync(m => m.Id == mediaId.Value && m.StationId == stationId);

                if (Media == null)
                {
                    return NotFound();
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int stationId, int? mediaId)
        {
            // Station laden
            Station = await _dbContext.Stations.FirstOrDefaultAsync(s => s.Id == stationId);
            if (Station == null)
            {
                return NotFound();
            }

            // Wenn eine Datei hochgeladen wurde, speichern wir sie und setzen die Url automatisch
            if (UploadFile != null && UploadFile.Length > 0)
            {
                // Dateiendung prüfen (einfache Whitelist)
                var extension = Path.GetExtension(UploadFile.FileName);
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp3", ".wav", ".mp4" };

                if (!allowedExtensions.Contains(extension.ToLower()))
                {
                    ModelState.AddModelError("UploadFile", "Der gewählte Dateityp ist nicht erlaubt.");
                }
                else
                {
                    // Zielordner: wwwroot/uploads/stations/{stationId}
                    var uploadsRootFolder = Path.Combine(
                        _env.WebRootPath,
                        "uploads",
                        "stations",
                        stationId.ToString());

                    Directory.CreateDirectory(uploadsRootFolder);

                    // Eindeutiger Dateiname
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsRootFolder, fileName);

                    // Datei auf Server speichern
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await UploadFile.CopyToAsync(stream);
                    }

                    // Relative URL, die von außen aufrufbar ist
                    var relativePath = $"/uploads/stations/{stationId}/{fileName}";

                    // Url-Feld aus dem Formular ist dann nicht mehr "wirklich" required,
                    // deshalb ersetzen wir es durch die automatisch gesetzte URL
                    Media.Url = relativePath;

                    // Validation-Eintrag für Media.Url entfernen, falls der leer war
                    ModelState.Remove("Media.Url");
                }
            }

            // Wenn weder Datei hochgeladen wurde noch eine URL angegeben ist,
            // greift weiter die [Required]-Validierung auf Media.Url
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Media.Id == 0)
            {
                // Neues Medium
                Media.StationId = stationId;
                _dbContext.MediaItems.Add(Media);
                TempData["SuccessMessage"] = "Medium wurde erfolgreich angelegt.";
            }
            else
            {
                // Bestehendes Medium aktualisieren
                _dbContext.Attach(Media).State = EntityState.Modified;
                TempData["SuccessMessage"] = "Medium wurde erfolgreich aktualisiert.";

            }

            await _dbContext.SaveChangesAsync();

            return RedirectToPage("ManageMedia", new { stationId = stationId });
        }

    }
}
