using System.Globalization;

namespace SpotiClone.Helpers;

public class DurationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int ms && ms > 0)
            return TimeSpan.FromMilliseconds(ms).ToString(@"m\:ss");
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
