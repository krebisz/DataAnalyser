using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DataVisualiser.UI.Charts.Converters;

public sealed class PercentOfTotalConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length == 0)
            return "Percent: n/a";

        if (!TryResolve(values, TryGetValue, out var value))
            return "Percent: n/a";

        if (!TryResolve(values, TryGetTotal, out var total) || total <= 0)
            return "Percent: n/a";

        var percent = value / total;
        return string.Format(culture, "Percent: {0:P1}", percent);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static bool TryResolve(object[] values, Func<object?, (bool Success, double Value)> resolver, out double value)
    {
        for (var i = 0; i < values.Length; i++)
        {
            var result = resolver(values[i]);
            if (result.Success)
            {
                value = result.Value;
                return true;
            }
        }

        value = 0;
        return false;
    }

    private static (bool Success, double Value) TryGetValue(object? value)
    {
        if (value is null || ReferenceEquals(value, DependencyProperty.UnsetValue))
        {
            return (false, 0);
        }

        switch (value)
        {
            case double d:
                return (true, d);
            case float f:
                return (true, f);
            case decimal m:
                return (true, (double)m);
            case int i:
                return (true, i);
            case long l:
                return (true, l);
            case string s when double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed):
                return (true, parsed);
        }

        var type = value.GetType();
        var valueProperty = type.GetProperty("Value");
        if (valueProperty != null)
        {
            var propertyValue = valueProperty.GetValue(value);
            return TryGetValue(propertyValue);
        }

        var itemProperty = type.GetProperty("Item");
        if (itemProperty != null)
        {
            var item = itemProperty.GetValue(value);
            return TryGetValue(item);
        }

        return (false, 0);
    }

    private static (bool Success, double Value) TryGetTotal(object? value)
    {
        if (value is null || ReferenceEquals(value, DependencyProperty.UnsetValue))
        {
            return (false, 0);
        }

        switch (value)
        {
            case double d:
                return (true, d);
            case float f:
                return (true, f);
            case decimal m:
                return (true, (double)m);
            case int i:
                return (true, i);
            case long l:
                return (true, l);
            case string s when double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed):
                return (true, parsed);
        }

        var type = value.GetType();
        var totalProperty = type.GetProperty("BucketTotal") ?? type.GetProperty("Total");
        if (totalProperty != null)
        {
            var propertyValue = totalProperty.GetValue(value);
            return TryGetTotal(propertyValue);
        }

        var itemProperty = type.GetProperty("Item");
        if (itemProperty != null)
        {
            var item = itemProperty.GetValue(value);
            return TryGetTotal(item);
        }

        return (false, 0);
    }
}
