using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Dtos;

namespace Windeck.Geschichtstour.Backend.Controllers
{
    /// <summary>
    /// Web-API-Controller für den Zugriff auf Stationen.
    /// Diese Endpunkte werden von der mobilen App verwendet.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Der DbContext wird per Dependency Injection bereitgestellt.
        /// </summary>
        public StationsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Liefert eine Liste aller Stationen mit Basisinformationen.
        /// Dieser Endpunkt kann z. B. für die Stationsliste in der App genutzt werden.
        /// GET: /api/stations
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StationDto>>> GetStations()
        {
            // Stationen inklusive Kategorie und Medien laden.
            var stations = await _dbContext.Stations
                .Include(s => s.Category)
                .Include(s => s.MediaItems)
                .ToListAsync();

            // In DTOs projizieren, damit die App nur die benötigten Felder sieht.
            var result = stations.Select(s => new StationDto
            {
                Id = s.Id,
                Code = s.Code,
                Title = s.Title,
                ShortDescription = s.ShortDescription,
                LongDescription = s.LongDescription,
                Street = s.Street,
                HouseNumber = s.HouseNumber,
                ZipCode = s.ZipCode,
                City = s.City,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                CategoryName = s.Category != null ? s.Category.Name : null,
                MediaItems = s.MediaItems
                    .OrderBy(m => m.SortOrder)
                    .Select(m => new MediaItemDto
                    {
                        Id = m.Id,
                        MediaType = m.MediaType,
                        Url = m.Url,
                        Caption = m.Caption,
                        SortOrder = m.SortOrder
                    })
                    .ToList()
            });

            return Ok(result);
        }

        /// <summary>
        /// Liefert eine einzelne Station anhand des Codes (z. B. QR-Code).
        /// GET: /api/stations/by-code/{code}
        /// </summary>
        [HttpGet("by-code/{code}")]
        public async Task<ActionResult<StationDto>> GetStationByCode(string code)
        {
            // Station inkl. Kategorie und Medien anhand des Codes suchen.
            var station = await _dbContext.Stations
                .Include(s => s.Category)
                .Include(s => s.MediaItems)
                .FirstOrDefaultAsync(s => s.Code == code);

            if (station == null)
            {
                // 404, falls der Code keiner Station entspricht.
                return NotFound();
            }

            var dto = new StationDto
            {
                Id = station.Id,
                Code = station.Code,
                Title = station.Title,
                ShortDescription = station.ShortDescription,
                LongDescription = station.LongDescription,
                Street = station.Street,
                HouseNumber = station.HouseNumber,
                ZipCode = station.ZipCode,
                City = station.City,
                Latitude = station.Latitude,
                Longitude = station.Longitude,
                CategoryName = station.Category != null ? station.Category.Name : null,
                MediaItems = station.MediaItems
                    .OrderBy(m => m.SortOrder)
                    .Select(m => new MediaItemDto
                    {
                        Id = m.Id,
                        MediaType = m.MediaType,
                        Url = m.Url,
                        Caption = m.Caption,
                        SortOrder = m.SortOrder
                    })
                    .ToList()
            };

            return Ok(dto);
        }
    }
}
