using System.Globalization;
using ZXing.Net.Maui;

namespace Windeck.Geschichtstour.Mobile.Helper;

public class BarcodeEventToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is BarcodeDetectionEventArgs e)
            return e.Results?.FirstOrDefault()?.Value;

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
