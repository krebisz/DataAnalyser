using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DataVisualiser.UI.Charts.Converters;

public sealed class FirstNonEmptyTextConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        foreach (var value in values)
        {
            if (value == null)
                continue;

            if (ReferenceEquals(value, DependencyProperty.UnsetValue))
                continue;

            if (value is string text && !string.IsNullOrWhiteSpace(text))
                return text;

            var asString = value.ToString();
            if (!string.IsNullOrWhiteSpace(asString))
                return asString;
        }

        return string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
