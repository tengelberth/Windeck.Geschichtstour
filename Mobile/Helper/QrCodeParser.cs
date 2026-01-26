namespace Windeck.Geschichtstour.Mobile.Helper;

public static class QrCodeParser
{
    // Akzeptiert z.B.
    // https://geschichtstour-backend.azurewebsites.net/station?code=BURG_WINDECK
    // oder auch direkt "BURG_WINDECK"
    public static string? TryExtractCode(string raw)
    {
        raw = raw?.Trim() ?? "";
        if (raw.Length == 0) return null;

        // Falls QR nur den Code enthält
        if (!raw.Contains("://", StringComparison.OrdinalIgnoreCase) && !raw.Contains("?", StringComparison.Ordinal))
            return raw;

        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri))
            return null;

        // Query manuell parsen (ohne System.Web)
        var query = uri.Query.TrimStart('?');
        if (query.Length == 0) return null;

        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2 && kv[0].Equals("code", StringComparison.OrdinalIgnoreCase))
                return Uri.UnescapeDataString(kv[1]);
        }

        return null;
    }
}
