using System.Globalization;
using Windeck.Geschichtstour.Mobile.Helpers;
using Windeck.Geschichtstour.Mobile.ViewModels;

namespace Windeck.Geschichtstour.Mobile.Views;

/// <summary>
/// Content-Seite einer Station.
/// Wird ausschließlich über einen Code (z. B. QR-Code) geöffnet.
/// </summary>
public partial class StationContentPage : ContentPage, IQueryAttributable
{
    private readonly StationContentViewModel _viewModel;

    /// <summary>
    /// Initialisiert eine neue Instanz von StationContentPage.
    /// </summary>
    public StationContentPage(StationContentViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Übernimmt Navigationsparameter und lädt die dazugehörigen Inhalte.
    /// </summary>
    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("code", out var codeObj) || codeObj is not string code || string.IsNullOrWhiteSpace(code))
        {
            await UiNotify.ToastAsync("Kein gültiger Code übergeben.");
            await Shell.Current.GoToAsync("..");
            return;
        }

        await _viewModel.LoadByCodeAsync(code);

        if (!_viewModel.HasStation)
        {
            await UiNotify.ToastAsync($"Keine Station mit Code '{code}' gefunden.");
            await Shell.Current.GoToAsync("..");
        }
    }

    /// <summary>
    /// Fängt Navigationsversuche aus der WebView ab:
    /// - <c>height:</c>-Callbacks zur dynamischen Höhenanpassung
    /// - externe Links, die über den System-Launcher geöffnet werden.
    /// </summary>
    private async void DescriptionWebView_Navigating(object? sender, WebNavigatingEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Url))
            return;

        if (e.Url.StartsWith("height:", StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;
            ApplyReportedHeight(e.Url);
            return;
        }

        string? targetUrl = null;

        if (e.Url.StartsWith("extlink:", StringComparison.OrdinalIgnoreCase))
        {
            targetUrl = Uri.UnescapeDataString(e.Url.Substring("extlink:".Length));
        }
        else if (Uri.TryCreate(e.Url, UriKind.Absolute, out var uri))
        {
            var scheme = uri.Scheme.ToLowerInvariant();
            if (scheme is "http" or "https" or "mailto" or "tel")
                targetUrl = e.Url;
        }

        if (string.IsNullOrWhiteSpace(targetUrl))
            return;

        // Gleiches Verhalten wie im Flyout: direkt über Launcher öffnen.
        e.Cancel = true;
        await Launcher.OpenAsync(targetUrl);
    }

    /// <summary>
    /// Übernimmt die vom HTML gemeldete Höhe und setzt sie als WebView-Höhe im ViewModel.
    /// </summary>
    private void ApplyReportedHeight(string rawHeightUrl)
    {
        var raw = rawHeightUrl.Substring("height:".Length).Trim().Replace(',', '.');
        if (!double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var height) || height <= 0)
            return;

        // Sicherheitsmarge + sinnvoller Bereich
        var computedHeight = Math.Clamp(height + 36, 120, 12000);
        _viewModel.LongDescriptionHeight = computedHeight;
    }
}
