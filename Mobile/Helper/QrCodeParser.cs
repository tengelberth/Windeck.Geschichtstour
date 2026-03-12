using System.Text;

namespace Windeck.Geschichtstour.Mobile.Helper;

/// <summary>
/// Extrahiert und normalisiert Stationscodes aus QR-Inhalten, Deeplink-URLs und manuellen Eingaben.
/// </summary>
public static class QrCodeParser
{
    // Akzeptiert z.B.
    // https://geschichtstour-backend.azurewebsites.net/station?code=BURG_WINDECK
    // oder auch direkt "BURG_WINDECK"

    /// <summary>
    /// Extrahiert, falls möglich, einen Stationscode aus dem QR-Inhalt.
    /// </summary>
    public static string? TryExtractCode(string raw)
    {
        raw = raw?.Trim() ?? string.Empty;
        if (raw.Length == 0)
        {
            return null;
        }

        // Falls QR nur den Code enthält
        if (!raw.Contains("://", StringComparison.OrdinalIgnoreCase) && !raw.Contains("?", StringComparison.Ordinal))
        {
            return raw;
        }

        if (!Uri.TryCreate(raw, UriKind.Absolute, out Uri? uri))
        {
            return null;
        }

        string query = uri.Query.TrimStart('?');
        if (query.Length == 0)
        {
            return null;
        }

        foreach (string part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] kv = part.Split('=', 2);
            if (kv.Length == 2 && kv[0].Equals("code", StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(kv[1]);
            }
        }

        return null;
    }

    /// <summary>
    /// Wandelt freie Eingaben wie "burg windeck" in den erwarteten Stationscode "BURG_WINDECK" um.
    /// </summary>
    public static string? TryNormalizeCode(string? raw)
    {
        string? extracted = string.IsNullOrWhiteSpace(raw) ? null : TryExtractCode(raw.Trim()) ?? raw.Trim();
        if (string.IsNullOrWhiteSpace(extracted))
        {
            return null;
        }

        string expanded = extracted
            .Replace("ä", "ae", StringComparison.OrdinalIgnoreCase)
            .Replace("ö", "oe", StringComparison.OrdinalIgnoreCase)
            .Replace("ü", "ue", StringComparison.OrdinalIgnoreCase)
            .Replace("ß", "ss", StringComparison.OrdinalIgnoreCase);

        StringBuilder builder = new(expanded.Length);
        bool lastWasSeparator = false;

        foreach (char ch in expanded)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToUpperInvariant(ch));
                lastWasSeparator = false;
                continue;
            }

            if (ch == '_' || ch == '-' || char.IsWhiteSpace(ch) || ch == '/')
            {
                if (builder.Length > 0 && !lastWasSeparator)
                {
                    builder.Append('_');
                    lastWasSeparator = true;
                }
            }
        }

        string normalized = builder.ToString().Trim('_');
        return normalized.Length == 0 ? null : normalized;
    }
}
