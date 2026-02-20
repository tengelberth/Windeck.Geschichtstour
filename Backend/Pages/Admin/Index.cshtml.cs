using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Windeck.Geschichtstour.Backend.Data;

namespace Windeck.Geschichtstour.Backend.Pages.Admin
{
    /// <summary>
    /// Liefert Kennzahlen und Auswertungen fuer das Admin-Dashboard.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        public IndexModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int StationCount { get; private set; }
        public int TourCount { get; private set; }
        public int CategoryCount { get; private set; }
        public int MediaCount { get; private set; }

        public int ApiCallCount { get; private set; }
        public int StationViewCount { get; private set; }

        public string SelectedPeriod { get; private set; } = "30";
        public string SelectedPeriodLabel { get; private set; } = "Letzte 30 Tage";

        public List<string> TimelineLabels { get; private set; } = new();
        public List<int> TimelineApiCalls { get; private set; } = new();
        public List<int> TimelineStationViews { get; private set; } = new();

        public List<TopStationUsageItem> TopStations { get; private set; } = new();

        public async Task OnGetAsync(string? zeitraum)
        {
            StationCount = await _dbContext.Stations.CountAsync();
            TourCount = await _dbContext.Tours.CountAsync();
            CategoryCount = await _dbContext.Categories.CountAsync();
            MediaCount = await _dbContext.MediaItems.CountAsync();

            var endDate = DateTime.UtcNow.Date;
            var startDate = await ResolveStartDateAsync(endDate, zeitraum);

            var analyticsBaseQuery = _dbContext.AnalyticsEvents
                .AsNoTracking()
                .Where(a => a.CreatedAtUtc >= startDate && a.CreatedAtUtc < endDate.AddDays(1));

            ApiCallCount = await analyticsBaseQuery.CountAsync(a => a.EventType == "api_call");
            StationViewCount = await analyticsBaseQuery.CountAsync(a => a.EventType == "station_view");

            var groupedPerDay = await analyticsBaseQuery
                .GroupBy(a => a.CreatedAtUtc.Date)
                .Select(g => new
                {
                    Day = g.Key,
                    ApiCalls = g.Count(x => x.EventType == "api_call"),
                    StationViews = g.Count(x => x.EventType == "station_view")
                })
                .ToListAsync();

            var groupedPerDayMap = groupedPerDay.ToDictionary(x => x.Day, x => x);

            for (var day = startDate; day <= endDate; day = day.AddDays(1))
            {
                TimelineLabels.Add(day.ToString("dd.MM."));

                if (groupedPerDayMap.TryGetValue(day, out var dayData))
                {
                    TimelineApiCalls.Add(dayData.ApiCalls);
                    TimelineStationViews.Add(dayData.StationViews);
                }
                else
                {
                    TimelineApiCalls.Add(0);
                    TimelineStationViews.Add(0);
                }
            }

            TopStations = await _dbContext.AnalyticsEvents
                .AsNoTracking()
                .Where(a => a.CreatedAtUtc >= startDate
                    && a.CreatedAtUtc < endDate.AddDays(1)
                    && a.EventType == "station_view"
                    && a.StationId != null)
                .GroupBy(a => new { a.StationId, a.StationCode, a.StationTitle })
                .Select(g => new TopStationUsageItem
                {
                    StationId = g.Key.StationId!.Value,
                    StationCode = g.Key.StationCode ?? string.Empty,
                    StationTitle = g.Key.StationTitle ?? "(ohne Titel)",
                    Views = g.Count()
                })
                .OrderByDescending(x => x.Views)
                .ThenBy(x => x.StationTitle)
                .Take(10)
                .ToListAsync();
        }

        private async Task<DateTime> ResolveStartDateAsync(DateTime endDate, string? zeitraum)
        {
            var selectedPeriod = (zeitraum ?? "30").Trim().ToLowerInvariant();

            switch (selectedPeriod)
            {
                case "7":
                    SelectedPeriod = "7";
                    SelectedPeriodLabel = "Letzte 7 Tage";
                    return endDate.AddDays(-6);
                case "90":
                    SelectedPeriod = "90";
                    SelectedPeriodLabel = "Letzte 90 Tage";
                    return endDate.AddDays(-89);
                case "all":
                    SelectedPeriod = "all";
                    SelectedPeriodLabel = "Gesamter Zeitraum";

                    var minEventDate = await _dbContext.AnalyticsEvents
                        .AsNoTracking()
                        .MinAsync(a => (DateTime?)a.CreatedAtUtc);

                    return minEventDate?.Date ?? endDate;
                default:
                    SelectedPeriod = "30";
                    SelectedPeriodLabel = "Letzte 30 Tage";
                    return endDate.AddDays(-29);
            }
        }

        public class TopStationUsageItem
        {
            public int StationId { get; set; }
            public string StationCode { get; set; } = string.Empty;
            public string StationTitle { get; set; } = string.Empty;
            public int Views { get; set; }
        }
    }
}
