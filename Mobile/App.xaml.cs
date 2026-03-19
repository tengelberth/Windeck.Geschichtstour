using Windeck.Geschichtstour.Mobile.Configuration;
using Windeck.Geschichtstour.Mobile.Services;
using Windeck.Geschichtstour.Mobile.Views;

namespace Windeck.Geschichtstour.Mobile
{
    /// <summary>
    /// Einstiegspunkt der MAUI-Anwendung und zentrale Deeplink-Verarbeitung.
    /// </summary>
    public partial class App : Application
    {
        private readonly AppUrlOptions _appUrlOptions;
        private readonly ApiClient _apiClient;
        private int _warmupInProgress;

        /// <summary>
        /// Initialisiert die Anwendung, setzt die Shell als Hauptseite und startet den stillen Backend-Warmup.
        /// </summary>
        /// <param name="serviceProvider">DI-Container zum Aufloesen von AppShell.</param>
        /// <param name="appUrlOptions">Konfiguration fuer gueltige Deeplink-Hosts.</param>
        /// <param name="apiClient">API-Client fuer den stillen readyz-Aufruf beim Start.</param>
        public App(IServiceProvider serviceProvider, AppUrlOptions appUrlOptions, ApiClient apiClient)
        {
            _appUrlOptions = appUrlOptions;
            _apiClient = apiClient;
            InitializeComponent();

            MainPage = serviceProvider.GetRequiredService<AppShell>();
            TriggerBackendWarmup();
        }

        /// <summary>
        /// Startet beim Zurueckkehren in die App erneut einen leichten Warmup, falls der Backend-Pfad wieder kalt wurde.
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();
            TriggerBackendWarmup();
        }

        /// <summary>
        /// Verarbeitet eingehende Deeplinks und navigiert zur passenden Seite.
        /// </summary>
        /// <param name="uri">Eingehender Deeplink.</param>
        /// <remarks>Akzeptiert nur Hosts aus der konfigurierten Allowlist.</remarks>
        protected override async void OnAppLinkRequestReceived(Uri uri)
        {
            base.OnAppLinkRequestReceived(uri);

            await Dispatcher.DispatchAsync(async () =>
            {
                if (!_appUrlOptions.AllowedDeepLinkHosts.Contains(uri.Host))
                {
                    return;
                }

                string path = uri.AbsolutePath.TrimEnd('/').ToLowerInvariant();

                if (path == "/station")
                {
                    string? code = GetQueryParam(uri, "code");
                    if (string.IsNullOrWhiteSpace(code))
                    {
                        return;
                    }

                    await Shell.Current.GoToAsync(
                        $"{nameof(StationContentPage)}?code={Uri.EscapeDataString(code)}");
                }
                else if (path == "/tour")
                {
                    string? idValue = GetQueryParam(uri, "id");
                    if (!int.TryParse(idValue, out int id))
                    {
                        return;
                    }

                    await Shell.Current.GoToAsync(
                        $"{nameof(TourTeaserPage)}?id={id}");
                }
            });
        }

        /// <summary>
        /// Stösst einen stillen Warmup-Lauf an, ohne die UI davon abhängig zu machen.
        /// </summary>
        private void TriggerBackendWarmup()
        {
            _ = WarmupBackendAsync();
        }

        /// <summary>
        /// Ruft den readyz-Endpunkt mit kurzem Timeout auf, um den ersten Datenbankzugriff beim aktiven Nutzer abzufedern.
        /// </summary>
        private async Task WarmupBackendAsync()
        {
            if (Interlocked.Exchange(ref _warmupInProgress, 1) == 1)
            {
                return;
            }

            try
            {
                using CancellationTokenSource cts = new(TimeSpan.FromSeconds(4));
                await _apiClient.PingReadyAsync(cts.Token);
            }
            catch
            {
                // Der Warmup bleibt bewusst still und darf die App nicht stoeren.
            }
            finally
            {
                Interlocked.Exchange(ref _warmupInProgress, 0);
            }
        }

        /// <summary>
        /// Liest einen Query-Parameter aus einer URI.
        /// </summary>
        /// <param name="uri">URI mit Query-String.</param>
        /// <param name="key">Name des gesuchten Parameters.</param>
        /// <returns>Wert des Parameters oder <c>null</c>, wenn der Parameter nicht vorhanden ist.</returns>
        private static string? GetQueryParam(Uri uri, string key)
        {
            string query = uri.Query.TrimStart('?');
            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }

            foreach (string part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] kv = part.Split('=', 2);
                if (kv.Length == 2 && kv[0].Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return Uri.UnescapeDataString(kv[1]);
                }
            }

            return null;
        }
    }
}
