using System;
using System.Windows.Controls;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.UI.Charts.Infrastructure;
using DataVisualiser.UI.State;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using WpfCartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.UI.Charts.Helpers;

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

        return ChartSeriesHelper.HasSeries(chart.Series);
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

    public static void ResetPolarFit(PolarChart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        chart.FitToBounds = true;
    }

    public static bool HasSeries(PolarChart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        return ChartSeriesHelper.HasSeries(chart.Series);
    }
}
