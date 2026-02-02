using System.Text.Json;

namespace Windeck.Geschichtstour.Mobile.Configuration;

/// <summary>
/// Stellt zentral die konfigurierten URLs fuer Backend, Sharing und Deeplinks bereit.
/// </summary>
public sealed class AppUrlOptions
{
    /// <summary>
    /// Basis-URL fuer API-Aufrufe und statische Inhalte.
    /// </summary>
    public required Uri BackendBaseUri { get; init; }

    /// <summary>
    /// Oeffentliche Basis-URL fuer Website-Links und Share-Links.
    /// </summary>
    public required Uri PublicBaseUri { get; init; }

    /// <summary>
    /// Gueltige Hosts fuer eingehende Deeplinks.
    /// </summary>
    public required HashSet<string> AllowedDeepLinkHosts { get; init; }
}

/// <summary>
/// Laedt URL-Einstellungen aus App-Konfiguration und optional aus Umgebungsvariablen.
/// </summary>
public static class AppUrlOptionsLoader
{
    private const string DefaultBaseUrl = "https://geschichtstour-backend.azurewebsites.net/";
    
    /// <summary>
    /// Laedt die URL-Konfiguration synchron fuer den App-Start.
    /// </summary>
    /// <returns>Vollstaendig validierte URL-Konfiguration.</returns>
    public static AppUrlOptions Load()
    {
        var fileConfig = LoadFileConfig();

        var backendBaseUrl =
            Environment.GetEnvironmentVariable("WINDECK_BACKEND_BASE_URL")
            ?? fileConfig?.Backend?.BaseUrl
            ?? DefaultBaseUrl;

        var publicBaseUrl =
            Environment.GetEnvironmentVariable("WINDECK_PUBLIC_BASE_URL")
            ?? fileConfig?.Backend?.PublicBaseUrl
            ?? backendBaseUrl;

        var allowedHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            NormalizeUri(backendBaseUrl).Host,
            NormalizeUri(publicBaseUrl).Host
        };

        if (fileConfig?.Backend?.AllowedDeepLinkHosts is { Count: > 0 })
        {
            foreach (var host in fileConfig.Backend.AllowedDeepLinkHosts.Where(h => !string.IsNullOrWhiteSpace(h)))
                allowedHosts.Add(host.Trim());
        }

        var envHosts = Environment.GetEnvironmentVariable("WINDECK_ALLOWED_DEEPLINK_HOSTS");
        if (!string.IsNullOrWhiteSpace(envHosts))
        {
            foreach (var host in envHosts.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                allowedHosts.Add(host);
        }

        return new AppUrlOptions
        {
            BackendBaseUri = NormalizeUri(backendBaseUrl),
            PublicBaseUri = NormalizeUri(publicBaseUrl),
            AllowedDeepLinkHosts = allowedHosts
        };
    }

    private static Uri NormalizeUri(string input)
    {
        if (!Uri.TryCreate(input, UriKind.Absolute, out var parsed))
            throw new InvalidOperationException($"Ungueltige URL-Konfiguration: '{input}'.");

        var normalized = parsed.ToString();
        if (!normalized.EndsWith('/'))
            normalized += "/";

        return new Uri(normalized, UriKind.Absolute);
    }

    private static AppSettingsRoot? LoadFileConfig()
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
            return JsonSerializer.Deserialize<AppSettingsRoot>(stream);
        }
        catch
        {
            return null;
        }
    }

    private sealed class AppSettingsRoot
    {
        public BackendSection? Backend { get; set; }
    }

    private sealed class BackendSection
    {
        public string? BaseUrl { get; set; }
        public string? PublicBaseUrl { get; set; }
        public List<string>? AllowedDeepLinkHosts { get; set; }
    }
}
