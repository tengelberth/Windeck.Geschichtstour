using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Dtos;
using Windeck.Geschichtstour.Backend.Models;
using Windeck.Geschichtstour.Backend.Services;

namespace Windeck.Geschichtstour.Backend.Controllers
{
    /// <summary>
    /// Web-API-Controller fÃ¼r den Zugriff auf Touren.
    /// Die mobile App kann damit z. B. eine Liste von Touren anzeigen
    /// und eine Tour im Detail laden, um eine Route zu bauen.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ToursController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IAnalyticsService _analyticsService;

        /// <summary>
        /// Initialisiert eine neue Instanz von ToursController.
        /// </summary>
        public ToursController(AppDbContext dbContext, IAnalyticsService analyticsService)
        {
            _dbContext = dbContext;
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Liefert eine Liste aller Touren
        /// GET: /api/tours
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TourDto>>> GetTours()
        {
            List<Tour> tours = await _dbContext.Tours
            .Include(t => t.Stops)
                .ThenInclude(ts => ts.Station)
            .ToListAsync();

            List<TourDto> result = tours.Select(t => new TourDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                TourLink = t.TourLink,
                Stops = t.Stops
                    .OrderBy(s => s.Order)
                    .Select(ts => new TourStopDto
                    {
                        Order = ts.Order,
                        StationId = ts.StationId,
                        StationCode = ts.Station?.Code ?? string.Empty,
                        StationTitle = ts.Station?.Title ?? string.Empty,
                        Latitude = ts.Station?.Latitude,
                        Longitude = ts.Station?.Longitude
                    })
                    .ToList()
            }).ToList();

            await _analyticsService.TrackApiCallAsync(
                HttpContext,
                "/api/tours",
                "api_call",
                StatusCodes.Status200OK);

            return Ok(result);
        }

        /// <summary>
        /// Liefert eine Tour mit allen Stops und den zugehÃ¶rigen Stationen.
        /// GET: /api/tours/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TourDto>> GetTourById(int id)
        {
            Tour? tour = await _dbContext.Tours
                .Include(t => t.Stops)
                    .ThenInclude(ts => ts.Station)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null)
            {
                await _analyticsService.TrackApiCallAsync(
                    HttpContext,
                    "/api/tours/{id}",
                    "api_call",
                    StatusCodes.Status404NotFound,
                    tourId: id);

                return NotFound();
            }

            TourDto dto = new()
            {
                Id = tour.Id,
                Title = tour.Title,
                Description = tour.Description,
                TourLink = tour.TourLink,
                Stops = tour.Stops
                    .OrderBy(ts => ts.Order)
                    .Select(ts => new TourStopDto
                    {
                        Order = ts.Order,
                        StationId = ts.StationId,
                        StationCode = ts.Station?.Code ?? string.Empty,
                        StationTitle = ts.Station?.Title ?? string.Empty,
                        Latitude = ts.Station?.Latitude,
                        Longitude = ts.Station?.Longitude
                    })
                    .ToList()
            };

            await _analyticsService.TrackApiCallAsync(
                HttpContext,
                "/api/tours/{id}",
                "tour_view",
                StatusCodes.Status200OK,
                tourId: id);

            return Ok(dto);
        }
    }
}

