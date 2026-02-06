using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using DataVisualiser.UI.Syncfusion;
using Syncfusion.UI.Xaml.SunburstChart;

namespace DataVisualiser.UI.Charts.Converters;

public sealed class SunburstTooltipPercentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SunburstItem directItem && directItem.BucketTotal > 0)
            return string.Format(culture, "Percent: {0:P1}", directItem.Value / directItem.BucketTotal);

        if (value is DependencyObject dependencyObject)
        {
            if (TryResolvePercentFromVisual(dependencyObject, out var percent))
                return string.Format(culture, "Percent: {0:P1}", percent);
        }

        if (TryResolvePercentFromObjectGraph(value, out var percentFromGraph))
            return string.Format(culture, "Percent: {0:P1}", percentFromGraph);

        return "Percent: n/a";
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static bool TryResolvePercentFromVisual(DependencyObject target, out double percent)
    {
        percent = 0;

        var chart = FindAncestorChart(target);
        if (chart == null)
            return false;

        if (chart.ItemsSource is not IEnumerable items)
            return false;

        var itemList = items.OfType<SunburstItem>().ToList();
        if (itemList.Count == 0)
            return false;

        var total = itemList.Sum(item => item.Value);
        if (total <= 0)
            return false;

        var submetric = ResolveCategory(target);
        if (string.IsNullOrWhiteSpace(submetric))
            return false;

        var value = itemList
            .Where(item => string.Equals(item.Submetric, submetric, StringComparison.OrdinalIgnoreCase))
            .Sum(item => item.Value);

        if (value <= 0)
            return false;

        percent = value / total;
        return true;
    }

    private static SfSunburstChart? FindAncestorChart(DependencyObject target)
    {
        var current = target;
        while (current != null)
        {
            if (current is SfSunburstChart chart)
                return chart;

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static string? ResolveCategory(object target)
    {
        var category = GetStringProperty(target, "Category");
        if (!string.IsNullOrWhiteSpace(category))
            return category;

        var item = GetPropertyValue(target, "Item");
        if (item is SunburstItem sunburstItem)
            return sunburstItem.Submetric;

        if (item != null)
        {
            category = GetStringProperty(item, "Category");
            if (!string.IsNullOrWhiteSpace(category))
                return category;

            category = GetStringProperty(item, "Submetric");
            if (!string.IsNullOrWhiteSpace(category))
                return category;
        }

        return null;
    }

    private static bool TryResolvePercentFromObjectGraph(object target, out double percent)
    {
        percent = 0;

        if (target is SunburstItem item)
        {
            if (item.BucketTotal <= 0)
                return false;

            percent = item.Value / item.BucketTotal;
            return true;
        }

        var nestedItem = GetPropertyValue(target, "Item") as SunburstItem;
        if (nestedItem != null && nestedItem.BucketTotal > 0)
        {
            percent = nestedItem.Value / nestedItem.BucketTotal;
            return true;
        }

        return false;
    }

    private static object? GetPropertyValue(object instance, string name)
    {
        var prop = instance.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (prop == null || prop.GetIndexParameters().Length > 0)
            return null;

        try
        {
            return prop.GetValue(instance);
        }
        catch
        {
            return null;
        }
    }

    private static string? GetStringProperty(object instance, string name)
    {
        var value = GetPropertyValue(instance, name);
        return value?.ToString();
    }
}
