using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using DataVisualiser.UI.Syncfusion;

namespace DataVisualiser.UI.Charts.Converters;

public sealed class SunburstPercentTextConverter : IValueConverter
{
    private static readonly string[] ItemPropertyNames =
    {
        "Item",
        "DataItem",
        "Data",
        "Source",
        "Segment",
        "Point",
        "Info",
        "ToolTipInfo",
        "ChartData"
    };

    private static readonly string[] ValuePropertyNames =
    {
        "Value",
        "ActualValue",
        "YValue",
        "Y"
    };

    private static readonly string[] TotalPropertyNames =
    {
        "BucketTotal",
        "Total",
        "Sum",
        "Aggregate"
    };

    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (TryGetPercent(value, new HashSet<object>(ReferenceEqualityComparer.Instance), 0, out var percent))
            return string.Format(culture, "Percent: {0:P1}", percent);

        return "Percent: n/a";
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static bool TryGetPercent(object? value, HashSet<object> visited, int depth, out double percent)
    {
        percent = 0;

        if (value is null || ReferenceEquals(value, DependencyProperty.UnsetValue))
            return false;

        if (value is SunburstItem item)
        {
            if (item.BucketTotal <= 0)
                return false;

            percent = item.Value / item.BucketTotal;
            return true;
        }

        if (!visited.Add(value))
            return false;

        if (depth > 4)
            return false;

        if (TryGetValueAndTotal(value, out var actualValue, out var total))
        {
            if (total <= 0)
                return false;

            percent = actualValue / total;
            return true;
        }

        var type = value.GetType();
        foreach (var name in ItemPropertyNames)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || prop.GetIndexParameters().Length > 0)
                continue;

            var nested = SafeGetValue(prop, value);
            if (nested == null)
                continue;

            if (TryGetPercent(nested, visited, depth + 1, out percent))
                return true;
        }

        if (value is IEnumerable enumerable)
        {
            foreach (var element in enumerable)
            {
                if (TryGetPercent(element, visited, depth + 1, out percent))
                    return true;
            }
        }

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetIndexParameters().Length > 0)
                continue;

            var nested = SafeGetValue(prop, value);
            if (nested == null)
                continue;

            if (TryGetPercent(nested, visited, depth + 1, out percent))
                return true;
        }

        return false;
    }

    private static bool TryGetValueAndTotal(object value, out double actualValue, out double total)
    {
        actualValue = 0;
        total = 0;

        if (TryGetNumericByNames(value, ValuePropertyNames, out actualValue) &&
            TryGetNumericByNames(value, TotalPropertyNames, out total))
        {
            return true;
        }

        return false;
    }

    private static bool TryGetNumericByNames(object value, IEnumerable<string> names, out double result)
    {
        result = 0;
        var type = value.GetType();

        foreach (var name in names)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || prop.GetIndexParameters().Length > 0)
                continue;

            var raw = SafeGetValue(prop, value);
            if (TryGetDouble(raw, out result))
                return true;
        }

        return false;
    }

    private static object? SafeGetValue(PropertyInfo prop, object instance)
    {
        try
        {
            return prop.GetValue(instance);
        }
        catch
        {
            return null;
        }
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
            default:
                result = 0;
                return false;
        }
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();

        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
