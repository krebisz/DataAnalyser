using System;
using System.Collections;
using System.Linq;
using System.Windows.Controls;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.UI.State;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using WpfCartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.UI.Charts.Presentation;

public static class ChartSurfaceHelper
{
    public static void ClearCartesian(WpfCartesianChart chart, ChartState state)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));
        if (state == null)
            throw new ArgumentNullException(nameof(state));

        ChartHelper.ClearChart(chart, state.ChartTimestamps);
    }

    public static void ResetZoom(WpfCartesianChart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        ChartUiHelper.ResetZoom(chart);
    }

    public static bool HasSeries(WpfCartesianChart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        return HasAnySeries(chart.Series);
    }

    public static void ClearPolar(PolarChart chart, Func<ToolTip?>? getTooltip)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        chart.Series = Array.Empty<ISeries>();
        chart.AngleAxes = Array.Empty<PolarAxis>();
        chart.RadiusAxes = Array.Empty<PolarAxis>();
        chart.Tag = null;

        var tooltip = getTooltip?.Invoke();
        if (tooltip != null)
            tooltip.IsOpen = false;
    }

    public static bool HasSeries(PolarChart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        return HasAnySeries(chart.Series);
    }

    private static bool HasAnySeries(IEnumerable? series)
    {
        return series != null && series.Cast<object>().Any();
    }
}
