using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Windeck.Geschichtstour.Mobile.Services;

/// <summary>
/// Speichert JSON-Antworten plattformunabhängig im AppData-Verzeichnis, damit Listen und Details beim nächsten Aufruf sofort verfügbar sind.
/// </summary>
public class JsonCacheService
{
    private readonly string _cacheDirectory;

    /// <summary>
    /// Initialisiert den Dateicache im plattformneutralen MAUI-AppData-Verzeichnis.
    /// </summary>
    public JsonCacheService()
    {
        _cacheDirectory = Path.Combine(FileSystem.AppDataDirectory, "cache", "json");
        Directory.CreateDirectory(_cacheDirectory);
    }

    /// <summary>
    /// Liest einen Cacheeintrag, sofern die Datei vorhanden und noch nicht abgelaufen ist.
    /// </summary>
    public async Task<T?> TryGetAsync<T>(string cacheKey, JsonTypeInfo<T> typeInfo, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        string path = GetCachePath(cacheKey);
        if (!File.Exists(path))
        {
            return default;
        }

        DateTimeOffset lastWriteUtc = File.GetLastWriteTimeUtc(path);
        if (DateTimeOffset.UtcNow - lastWriteUtc > ttl)
        {
            return default;
        }

        try
        {
            await using FileStream stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync(stream, typeInfo, cancellationToken);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Persistiert einen Cacheeintrag als JSON-Datei.
    /// </summary>
    public async Task SetAsync<T>(string cacheKey, T value, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default)
    {
        string path = GetCachePath(cacheKey);
        string tempPath = path + ".tmp";

        Directory.CreateDirectory(_cacheDirectory);

        await using (FileStream stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, value, typeInfo, cancellationToken);
        }

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        File.Move(tempPath, path);
    }

    /// <summary>
    /// Erzeugt einen stabilen Dateinamen aus dem Cache-Key, ohne plattformspezifische Sonderzeichen zu riskieren.
    /// </summary>
    private string GetCachePath(string cacheKey)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(cacheKey);
        byte[] hashBytes = SHA256.HashData(keyBytes);
        string hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
        return Path.Combine(_cacheDirectory, hash + ".json");
    }
}
