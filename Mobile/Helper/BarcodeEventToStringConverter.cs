using System.Globalization;
using ZXing.Net.Maui;

namespace Windeck.Geschichtstour.Mobile.Helper;

/// <summary>
/// Konvertiert Barcode-Events in anzeigbaren Text fuer XAML-Bindings.
/// </summary>
public class BarcodeEventToStringConverter : IValueConverter
{
    /// <summary>
    /// Konvertiert den eingehenden Wert in eine fuer die UI geeignete Darstellung.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is BarcodeDetectionEventArgs e)
            return e.Results?.FirstOrDefault()?.Value;

        return null;
    }

    /// <summary>
    /// Konvertiert einen UI-Wert in den erwarteten Quelltyp zurueck.
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}


