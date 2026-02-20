namespace Windeck.Geschichtstour.Backend.Services
{
    public interface IAnalyticsService
    {
        Task TrackApiCallAsync(
            HttpContext httpContext,
            string endpoint,
            string eventType,
            int statusCode,
            int? stationId = null,
            string? stationCode = null,
            string? stationTitle = null,
            int? tourId = null);
    }
}
