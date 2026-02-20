using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Models;

namespace Windeck.Geschichtstour.Backend.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(AppDbContext dbContext, ILogger<AnalyticsService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task TrackApiCallAsync(
            HttpContext httpContext,
            string endpoint,
            string eventType,
            int statusCode,
            int? stationId = null,
            string? stationCode = null,
            string? stationTitle = null,
            int? tourId = null)
        {
            try
            {
                var userAgent = httpContext.Request.Headers.UserAgent.ToString();

                var analyticsEvent = new AnalyticsEvent
                {
                    Endpoint = endpoint,
                    EventType = eventType,
                    StatusCode = statusCode,
                    StationId = stationId,
                    StationCode = stationCode,
                    StationTitle = stationTitle,
                    TourId = tourId,
                    UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent[..Math.Min(userAgent.Length, 300)],
                    CreatedAtUtc = DateTime.UtcNow
                };

                _dbContext.AnalyticsEvents.Add(analyticsEvent);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Analytics darf nie den eigentlichen API-Request verhindern.
                _logger.LogWarning(ex, "Analytics-Ereignis konnte nicht gespeichert werden.");
            }
        }
    }
}
