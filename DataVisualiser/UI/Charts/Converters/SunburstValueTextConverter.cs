using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DataVisualiser.UI.Charts.Converters;

public sealed class SunburstValueTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (TryGetDouble(value, out var result))
            return string.Format(culture, "Value: {0:N2}", result);

        return "Value: n/a";
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static bool TryGetDouble(object? value, out double result)
    {
        if (value is null || ReferenceEquals(value, DependencyProperty.UnsetValue))
        {
            result = 0;
            return false;
        }

        switch (value)
        {
            case double d:
                result = d;
                return true;
            case float f:
                result = f;
                return true;
            case decimal m:
                result = (double)m;
                return true;
            case int i:
                result = i;
                return true;
            case long l:
                result = l;
                return true;
            case string s when double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed):
                result = parsed;
                return true;
        }

        var type = value.GetType();
        var property = type.GetProperty("Value") ?? type.GetProperty("Item") ?? type.GetProperty("Total");
        if (property != null)
        {
            var propertyValue = property.GetValue(value);
            return TryGetDouble(propertyValue, out result);
        }

        result = 0;
        return false;
    }
}
