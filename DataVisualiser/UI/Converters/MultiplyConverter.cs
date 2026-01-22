using System.Globalization;
using System.Windows.Data;

namespace DataVisualiser.UI.Converters;

public sealed class MultiplyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double input)
            return Binding.DoNothing;

        var factor = 1.0;

        if (parameter is double d)
            factor = d;
        else if (parameter is string s && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            factor = parsed;

        return input * factor;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}