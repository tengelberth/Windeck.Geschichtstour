using System.Globalization;
using Windeck.Geschichtstour.Mobile.Configuration;
using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.Models;
using Windeck.Geschichtstour.Mobile.Services;
using Windeck.Geschichtstour.Mobile.Views;

namespace Windeck.Geschichtstour.Mobile.ViewModels;

/// <summary>
/// Lädt und präsentiert Detailinformationen zu einer ausgewählten Tour.
/// </summary>
public class TourTeaserViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private readonly AppUrlOptions _appUrlOptions;

    private TourDto? _tour;

    public IEnumerable<TourStopDto> OrderedStops =>
        Tour?.Stops?
            .OrderBy(s => s.Order)
        ?? Enumerable.Empty<TourStopDto>();

    public TourDto? Tour
    {
        get => _tour;
        set
        {
            if (SetProperty(ref _tour, value))
            {
                OnPropertyChanged(nameof(HasTour));
                OnPropertyChanged(nameof(HasTourLink));
                OnPropertyChanged(nameof(OrderedStops));
                OnPropertyChanged(nameof(StopCountText));
            }
        }
    }

    public bool HasTour => Tour != null;
    public bool HasTourLink => !string.IsNullOrWhiteSpace(Tour?.TourLink);
    public string StopCountText => Tour == null ? string.Empty : $"{Tour.Stops.Count} Stationen";

    public Command StartTourNavigationCommand { get; }
    public Command StartTourLinkCommand { get; }
    public Command<TourStopDto> OpenTourStopCommand { get; }
    public Command ShareTourCommand { get; }

    /// <summary>
    /// Initialisiert eine neue Instanz von TourTeaserViewModel.
    /// </summary>
    public TourTeaserViewModel(ApiClient apiClient, AppUrlOptions appUrlOptions)
    {
        _apiClient = apiClient;
        _appUrlOptions = appUrlOptions;
        StartTourNavigationCommand = new Command(async () => await OpenTourInMapsAsync());
        StartTourLinkCommand = new Command(async () => await OpenTourLinkAsync());
        OpenTourStopCommand = new Command<TourStopDto>(async stop => await OpenTourStopAsync(stop));
        ShareTourCommand = new Command(async () => await ShareTourAsync());
    }

    /// <summary>
    /// Lädt einen Datensatz anhand der übergebenen ID.
    /// </summary>
    public async Task LoadByIdAsync(int id)
    {
        if (IsBusy)
        {
            return;
        }

        bool loadedFromCache = false;

        try
        {
            TourDto? cachedTour = await _apiClient.TryGetCachedTourByIdAsync(id);
            if (cachedTour != null)
            {
                Tour = cachedTour;
                loadedFromCache = true;
            }

            if (!loadedFromCache)
            {
                IsBusy = true;
                StartLoadingFeedback(
                    "Hoppla, da hast du uns wohl beim Nickerchen erwischt.",
                    "Wir wecken gerade kurz den Server auf.",
                    "Im Hintergrund fährt jetzt auch die Datenbank hoch.",
                    "Das dauert einen kleinen Moment. So sparen wir jedoch laufende Kosten.",
                    "Sobald der Server wach ist, werden die nächsten Anfragen deutlich schneller bearbeitet.",
                    "Fast da - wir sammeln die Inhalte gerade für dich zusammen.",
                "Dir gefällt die App oder du hast Verbesserungsvorschläge? Dann schreib uns gern eine Rezension im Store.",
                "Du findest, es fehlen noch Stationen? Dann melde dich und gestalte die Inhalte mit.");
                Tour = null;
            }

            TourDto? tour = await _apiClient.GetTourByIdAsync(id, allowUserRetryUi: false);
            if (tour != null || !loadedFromCache)
            {
                Tour = tour;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Laden der Tour {id}: {ex}");
            if (!loadedFromCache)
            {
                Tour = null;
            }
        }
        finally
        {
            if (!loadedFromCache)
            {
                StopLoadingFeedback();
                IsBusy = false;
            }
        }
    }

    /// <summary>
    /// Öffnet den optional hinterlegten externen Tour-Link.
    /// </summary>
    private async Task OpenTourLinkAsync()
    {
        if (!TryGetTourLinkUri(out Uri? uri))
        {
            await UiNotify.ToastAsync("Kein gültiger Tour-Link hinterlegt.");
            return;
        }

        await Launcher.OpenAsync(uri!);
    }

    /// <summary>
    /// Normalisiert den externen Tour-Link zu einer absoluten HTTP(S)-URL.
    /// </summary>
    private bool TryGetTourLinkUri(out Uri? uri)
    {
        uri = null;

        string? rawUrl = Tour?.TourLink?.Trim();
        if (string.IsNullOrWhiteSpace(rawUrl))
        {
            return false;
        }

        if (!rawUrl.Contains("://", StringComparison.Ordinal))
        {
            rawUrl = $"https://{rawUrl}";
        }

        if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out uri))
        {
            return false;
        }

        return uri != null && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
    /// <summary>
    /// Öffnet die ausgewählte Tour in der Karten-App.
    /// </summary>
    private async Task OpenTourInMapsAsync()
    {
        if (Tour == null)
        {
            return;
        }

        List<TourStopDto> stopsWithCoords = Tour.Stops
            .Where(s => s != null && s.Latitude.HasValue && s.Longitude.HasValue)
            .OrderBy(s => s.Order)
            .ToList();

        if (stopsWithCoords.Count < 2)
        {
            await UiNotify.ToastAsync("Keine Tourdaten zum Berechnen verf�gbar.");
            return;
        }

        TourStopDto destination = stopsWithCoords.Last()!;
        List<TourStopDto> waypoints = stopsWithCoords.SkipLast(1).ToList();

        string FormatCoords(TourStopDto t) =>
            $"{t.Latitude!.Value.ToString(CultureInfo.InvariantCulture)},{t.Longitude!.Value.ToString(CultureInfo.InvariantCulture)}";

        string destStr = FormatCoords(destination);
        string url = $"https://www.google.com/maps/dir/?api=1&destination={destStr}";

        if (waypoints.Any())
        {
            string wp = string.Join("|", waypoints.Select(FormatCoords));
            url += $"&waypoints={wp}";
        }

        await Launcher.OpenAsync(url);
    }

    /// <summary>
    /// Öffnet den Inhalt einer Station direkt aus der Tourliste.
    /// </summary>
    private async Task OpenTourStopAsync(TourStopDto? stop)
    {
        if (string.IsNullOrWhiteSpace(stop?.StationCode))
        {
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(StationContentPage)}?code={Uri.EscapeDataString(stop.StationCode)}");
    }

    /// <summary>
    /// Erstellt einen öffentlichen Share-Link zur aktuellen Tour.
    /// </summary>
    private async Task ShareTourAsync()
    {
        if (Tour == null)
        {
            return;
        }

        string url = new Uri(_appUrlOptions.PublicBaseUri, $"tour?id={Tour.Id}").ToString();

        await Share.RequestAsync(new ShareTextRequest
        {
            Title = Tour.Title,
            Text = "Schau dir diese Tour an:",
            Uri = url
        });
    }
}
