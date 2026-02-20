using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Dtos;
using Windeck.Geschichtstour.Backend.Services;

namespace Windeck.Geschichtstour.Backend.Controllers
{
    /// <summary>
    /// Web-API-Controller für den Zugriff auf Touren.
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
            var tours = await _dbContext.Tours
            .Include(t => t.Stops)
                .ThenInclude(ts => ts.Station)
            .ToListAsync();

            var result = tours.Select(t => new TourDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Stops = t.Stops
                    .OrderBy(s => s.Order)
                    .Select(ts => new TourStopDto
                    {
                        Order = ts.Order,
                        StationId = ts.StationId,
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
        /// Liefert eine Tour mit allen Stops und den zugehörigen Stationen.
        /// GET: /api/tours/{id}
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TourDto>> GetTourById(int id)
        {
            var tour = await _dbContext.Tours
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

            var dto = new TourDto
            {
                Id = tour.Id,
                Title = tour.Title,
                Description = tour.Description,
                Stops = tour.Stops
                    .OrderBy(ts => ts.Order)
                    .Select(ts => new TourStopDto
                    {
                        Order = ts.Order,
                        StationId = ts.StationId,
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
