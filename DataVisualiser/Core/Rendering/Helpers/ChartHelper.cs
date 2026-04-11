using System.Windows.Media;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using LiveCharts;
using LiveCharts.Wpf;
using Separator = LiveCharts.Wpf.Separator;

namespace DataVisualiser.Core.Rendering.Helpers;

public static class ChartHelper
{
    public static LineSeries? CreateLineSeries(string title, int pointSize, int lineThickness, Color colour, bool dataLabels = false)
    {
        var smoothedSeries = new LineSeries
        {
                Title = title,
                Values = new ChartValues<double>(),
                PointGeometrySize = pointSize,
                StrokeThickness = lineThickness,
                Fill = Brushes.Transparent,
                Stroke = new SolidColorBrush(colour),
                DataLabels = dataLabels
        };

        return smoothedSeries;
    }

    public static string GetChartValuesAtIndex(CartesianChart chart, int index)
    {
        return ChartTooltipFormattingHelper.GetChartValuesAtIndex(chart, index);
    }

    public static string GetChartValuesFormattedAtIndex(CartesianChart chart, int index)
    {
        return ChartTooltipFormattingHelper.GetChartValuesFormattedAtIndex(chart, index);
    }

    public static void ClearChart(CartesianChart? chart, Dictionary<CartesianChart, List<DateTime>>? chartTimestamps = null)
    {
        if (chart == null)
            return;

        chart.Series.Clear();
        chartTimestamps?.Remove(chart);
    }

    public static void InitializeChartTooltip(CartesianChart chart)
    {
        if (chart == null)
            return;

        if (chart.DataTooltip is not SimpleChartTooltip)
            chart.DataTooltip = new SimpleChartTooltip();
    }

    public static string GetTimestampTextForIndex(int index, Dictionary<CartesianChart, List<DateTime>> chartTimestamps, params CartesianChart[] charts)
    {
        foreach (var chart in charts)
            if (chartTimestamps.TryGetValue(chart, out var list) && index >= 0 && index < list.Count)
                return list[index].ToString("yyyy-MM-dd HH:mm:ss");

        return "Timestamp: N/A";
    }

    public static void NormalizeYAxis(Axis yAxis, List<MetricData> rawData, List<double> smoothedValues)
    {
        if (!TransformChartAxisCalculator.TryCreateYAxisLayout(rawData, smoothedValues, out var layout))
        {
            yAxis.MinValue = double.NaN;
            yAxis.MaxValue = double.NaN;
            yAxis.Separator = new Separator();
            yAxis.ShowLabels = false;
            yAxis.Labels = null;
            return;
        }

        yAxis.MinValue = layout.MinValue;
        yAxis.MaxValue = layout.MaxValue;
        yAxis.Separator = layout.Step.HasValue ? new Separator { Step = layout.Step.Value } : new Separator();
        yAxis.LabelFormatter = value => MathHelper.FormatDisplayedValue(value);
        yAxis.ShowLabels = layout.ShowLabels;
        yAxis.Labels = null;
    }

    public static void ApplyTransformChartGradient(CartesianChart chart, Axis yAxis)
    {
        if (chart == null || yAxis == null)
            return;

        if (!string.Equals(chart.Name, "ChartTransformResultControl", StringComparison.Ordinal))
            return;

        if (double.IsNaN(yAxis.MinValue) || double.IsNaN(yAxis.MaxValue))
            return;

        var gradient = CreateYAxisGradientBrush(yAxis.MinValue, yAxis.MaxValue);
        if (gradient == null)
            return;

        foreach (var series in chart.Series.OfType<LineSeries>())
        {
            var (_, _, isSmoothed) = ChartTooltipFormattingHelper.ParseSeriesTitle(series.Title ?? string.Empty);
            if (isSmoothed)
                continue;

            series.Stroke = gradient;
        }
    }

    private static LinearGradientBrush? CreateYAxisGradientBrush(double minValue, double maxValue)
    {
        var range = maxValue - minValue;
        if (range <= 0 || double.IsNaN(range) || double.IsInfinity(range))
            return null;

        var midValue = minValue < 0 && maxValue > 0 ? 0 : minValue + range / 2.0;
        var midOffset = (midValue - minValue) / range;

        var brush = new LinearGradientBrush
        {
                StartPoint = new System.Windows.Point(0, 1),
                EndPoint = new System.Windows.Point(0, 0),
                SpreadMethod = GradientSpreadMethod.Pad
        };

        brush.GradientStops.Add(new GradientStop(Color.FromRgb(0x2B, 0x6C, 0xB0), 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromRgb(0xE9, 0xC4, 0x6A), midOffset));
        brush.GradientStops.Add(new GradientStop(Color.FromRgb(0xD6, 0x28, 0x28), 1.0));

        return brush;
    }

    public static void AdjustChartHeightBasedOnYAxis(CartesianChart chart, double minHeight)
    {
        if (chart == null || chart.AxisY.Count == 0)
            return;

        var yAxis = chart.AxisY[0];
        chart.Height = TransformChartAxisCalculator.CalculateChartHeight(yAxis.MinValue, yAxis.MaxValue, yAxis.Separator?.Step, minHeight);
    }
}
