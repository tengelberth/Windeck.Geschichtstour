using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Models;

namespace Windeck.Geschichtstour.Backend.Pages.Admin.Stations
{
    /// <summary>
    /// Anlegen und Bearbeiten eines MediaItems zu einer Station.
    /// </summary>
    public class EditMediaModel : PageModel
    {
        private const int MaxUploadSizeBytes = 500 * 1024;

        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Initialisiert eine neue Instanz von EditMediaModel.
        /// </summary>
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

        /// <summary>
        /// Lädt die für die Seite benötigten Daten bei einer GET-Anfrage.
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int stationId, int? mediaId)
        {
            Station = await _dbContext.Stations.FirstOrDefaultAsync(s => s.Id == stationId);
            if (Station == null)
            {
                return NotFound();
            }

            if (mediaId == null)
            {
                var nextSortOrder = await GetNextSortOrderAsync(stationId);
                Media = new MediaItem
                {
                    StationId = stationId,
                    MediaType = "Image",
                    SortOrder = nextSortOrder
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

            // Aktuell werden in der Admin-Oberfläche nur Bilder unterstützt.
            Media.MediaType = "Image";

            return Page();
        }

        /// <summary>
        /// Verarbeitet das Absenden des Formulars und speichert Änderungen.
        /// </summary>
        public async Task<IActionResult> OnPostAsync(int stationId, int? mediaId)
        {
            Station = await _dbContext.Stations.FirstOrDefaultAsync(s => s.Id == stationId);
            if (Station == null)
            {
                return NotFound();
            }

            if (Media == null)
            {
                return BadRequest();
            }

            if (UploadFile != null && UploadFile.Length > 0)
            {
                var extension = Path.GetExtension(UploadFile.FileName).ToLowerInvariant();
                var allowedExtensions = new[]
                {
                    ".jpg", ".jpeg", ".png", ".gif"
                    // ".mp3", ".wav", ".mp4"
                };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("UploadFile", "Der gewählte Dateityp ist nicht erlaubt.");
                }
                else
                {
                    var uploadsRootFolder = Path.Combine(
                        _env.WebRootPath,
                        "uploads",
                        "stations",
                        stationId.ToString());

                    Directory.CreateDirectory(uploadsRootFolder);

                    try
                    {
                        var compressed = await CompressImageAsync(UploadFile, extension);
                        var fileName = $"{Guid.NewGuid()}{compressed.FileExtension}";
                        var filePath = Path.Combine(uploadsRootFolder, fileName);

                        await System.IO.File.WriteAllBytesAsync(filePath, compressed.Content);

                        Media.Url = $"/uploads/stations/{stationId}/{fileName}";
                        ModelState.Remove("Media.Url");
                    }
                    catch
                    {
                        ModelState.AddModelError("UploadFile", "Das Bild konnte nicht verarbeitet werden.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Media.Id == 0)
            {
                Media.StationId = stationId;
                _dbContext.MediaItems.Add(Media);
                TempData["SuccessMessage"] = "Medium wurde erfolgreich angelegt.";
            }
            else
            {
                _dbContext.Attach(Media).State = EntityState.Modified;
                TempData["SuccessMessage"] = "Medium wurde erfolgreich aktualisiert.";
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToPage("ManageMedia", new { stationId });
        }

        /// <summary>
        /// Ermittelt die nächste Sortierreihenfolge auf Basis der Anzahl vorhandener Medien.
        /// </summary>
        private async Task<int> GetNextSortOrderAsync(int stationId)
        {
            var mediaCount = await _dbContext.MediaItems
                .CountAsync(m => m.StationId == stationId);

            return mediaCount + 1;
        }

        /// <summary>
        /// Komprimiert ein hochgeladenes Bild ohne Änderung der Auflösung.
        /// </summary>
        private static async Task<CompressedImageResult> CompressImageAsync(IFormFile file, string originalExtension)
        {
            await using var inputStream = file.OpenReadStream();
            using var image = await Image.LoadAsync<Rgba32>(inputStream);

            var hasTransparency = HasTransparency(image);
            var targetExtension = ResolveTargetExtension(originalExtension, hasTransparency);

            if (targetExtension == ".jpg")
            {
                if (hasTransparency)
                {
                    image.Mutate(ctx => ctx.BackgroundColor(Color.White));
                }

                var jpegResult = await EncodeJpegWithTargetSizeAsync(image, MaxUploadSizeBytes);
                return new CompressedImageResult(jpegResult, ".jpg");
            }

            if (targetExtension == ".png")
            {
                var pngResult = await EncodePngWithTargetSizeAsync(image, MaxUploadSizeBytes);
                return new CompressedImageResult(pngResult, ".png");
            }

            await using var rawStream = file.OpenReadStream();
            using var buffer = new MemoryStream();
            await rawStream.CopyToAsync(buffer);
            return new CompressedImageResult(buffer.ToArray(), originalExtension);
        }

        /// <summary>
        /// Kodiert ein Bild als JPEG und reduziert die Qualität schrittweise bis zur Zielgröße.
        /// </summary>
        private static async Task<byte[]> EncodeJpegWithTargetSizeAsync(Image<Rgba32> image, int targetBytes)
        {
            var qualities = new[] { 90, 82, 75, 68, 60, 52, 45, 38, 32 };
            byte[]? smallest = null;

            foreach (var quality in qualities)
            {
                var current = await SaveToBytesAsync(image, new JpegEncoder { Quality = quality });

                if (smallest == null || current.Length < smallest.Length)
                {
                    smallest = current;
                }

                if (current.Length <= targetBytes)
                {
                    return current;
                }
            }

            return smallest ?? Array.Empty<byte>();
        }

        /// <summary>
        /// Kodiert ein Bild als PNG mit reduzierter Farbpalette.
        /// </summary>
        private static async Task<byte[]> EncodePngWithTargetSizeAsync(Image<Rgba32> image, int targetBytes)
        {
            var colors = new[] { 256, 192, 128, 96, 64, 48, 32 };
            byte[]? smallest = null;

            foreach (var colorCount in colors)
            {
                using var candidate = image.CloneAs<Rgba32>();
                candidate.Mutate(ctx => ctx.Quantize(new WuQuantizer(new QuantizerOptions
                {
                    MaxColors = colorCount
                })));

                var current = await SaveToBytesAsync(candidate, new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.BestCompression,
                    ColorType = PngColorType.Palette,
                    BitDepth = PngBitDepth.Bit8
                });

                if (smallest == null || current.Length < smallest.Length)
                {
                    smallest = current;
                }

                if (current.Length <= targetBytes)
                {
                    return current;
                }
            }

            return smallest ?? Array.Empty<byte>();
        }

        /// <summary>
        /// Prüft, ob das Bild transparente Pixel enthält.
        /// </summary>
        private static bool HasTransparency(Image<Rgba32> image)
        {
            var hasTransparency = false;

            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < accessor.Height && !hasTransparency; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (var x = 0; x < row.Length; x++)
                    {
                        if (row[x].A < 255)
                        {
                            hasTransparency = true;
                            break;
                        }
                    }
                }
            });

            return hasTransparency;
        }

        /// <summary>
        /// Legt fest, ob als JPEG oder PNG gespeichert wird.
        /// </summary>
        private static string ResolveTargetExtension(string originalExtension, bool hasTransparency)
        {
            return originalExtension switch
            {
                ".jpg" => ".jpg",
                ".jpeg" => ".jpg",
                ".png" when hasTransparency => ".png",
                ".png" => ".jpg",
                ".gif" when hasTransparency => ".png",
                ".gif" => ".jpg",
                _ => originalExtension
            };
        }

        /// <summary>
        /// Speichert ein Bild mit dem übergebenen Encoder in ein Byte-Array.
        /// </summary>
        private static async Task<byte[]> SaveToBytesAsync(Image image, IImageEncoder encoder)
        {
            await using var ms = new MemoryStream();
            await image.SaveAsync(ms, encoder);
            return ms.ToArray();
        }

        private readonly record struct CompressedImageResult(byte[] Content, string FileExtension);
    }
}
