using System.Windows;
using System.Windows.Media;
using LiveCharts.Wpf;
using Separator = LiveCharts.Wpf.Separator;

namespace DataVisualiser.Core.Rendering.Helpers;

public static class ChartThemeStylingHelper
{
    public static void ApplyCartesianChartTheme(CartesianChart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        foreach (var axis in chart.AxisX)
            ApplyAxisTheme(axis);

        foreach (var axis in chart.AxisY)
            ApplyAxisTheme(axis);
    }

    public static void ApplyAxisTheme(Axis axis)
    {
        ArgumentNullException.ThrowIfNull(axis);

        var axisBrush = ResolveBrush("ThemeChartAxisBrush", Brushes.Black);
        var gridBrush = ResolveBrush("ThemeChartGridLineBrush", axisBrush);

        axis.Foreground = axisBrush;
        axis.Separator ??= new Separator();
        axis.Separator.Stroke = gridBrush;
    }

    public static Brush ResolveBrush(string resourceKey, Brush fallback)
    {
        if (Application.Current?.TryFindResource(resourceKey) is Brush brush)
            return brush;

        return fallback;
    }
}
