using System.Globalization;
using System.Windows.Media;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class CartesianChartPresentationBuilder
{
    public static UiChartRenderModel CreateEmpty(string xTitle = "Timestamp", string yTitle = "Value") =>
        new()
        {
            AxesX =
            [
                new ChartAxisModel
                {
                    Title = xTitle,
                    Labels = [],
                    Min = 0,
                    Max = 1,
                    Step = 1,
                    ShowLabels = false
                }
            ],
            AxesY =
            [
                CreateFixedValueAxis(yTitle, -0.2d, 0.2d, 0.1d)
            ]
        };

    public static UiChartRenderModel CreateComputedSeries(
        ComputedSeriesResult computedSeries,
        Color rawColor,
        Color smoothedColor)
    {
        ArgumentNullException.ThrowIfNull(computedSeries);

        var series = new List<ChartSeriesModel>
        {
            new()
            {
                Name = "Raw",
                SeriesType = ChartSeriesType.Line,
                Values = computedSeries.RawValues.Select(NormalizeValue).Select(value => (double?)value).ToArray(),
                Color = rawColor
            }
        };

        if (computedSeries.SmoothedValues.Any(double.IsFinite))
        {
            series.Add(new ChartSeriesModel
            {
                Name = "Smoothed",
                SeriesType = ChartSeriesType.Line,
                Values = computedSeries.SmoothedValues.Select(NormalizeValue).Select(value => (double?)value).ToArray(),
                Color = smoothedColor
            });
        }

        return new UiChartRenderModel
        {
            Series = series,
            AxesX =
            [
                CreateCategoryAxis("Timestamp", computedSeries.Timeline.Select(FormatTimestamp).ToArray())
            ],
            AxesY =
            [
                CreateValueAxis(
                    computedSeries.Label,
                    computedSeries.RawValues.Select(NormalizeValue),
                    computedSeries.SmoothedValues.Select(NormalizeValue))
            ]
        };
    }

    public static ChartAxisModel CreateCategoryAxis(
        string title,
        IReadOnlyList<string> labels,
        bool showLabels = true)
    {
        var count = labels.Count;
        return new ChartAxisModel
        {
            Title = title,
            Labels = labels,
            Min = 0,
            Max = Math.Max(0, count - 1),
            Step = ResolveCategoryStep(count),
            ShowLabels = showLabels && count > 0
        };
    }

    public static ChartAxisModel CreateValueAxis(
        string title,
        IEnumerable<double> rawValues,
        IEnumerable<double> smoothedValues)
    {
        var rawData = rawValues
            .Where(value => double.IsFinite(value) && Math.Abs(value) <= (double)decimal.MaxValue)
            .Select(value => new MetricData { Value = (decimal)value })
            .ToList();

        var smoothed = smoothedValues
            .Where(double.IsFinite)
            .ToList();

        if (!TransformChartAxisCalculator.TryCreateYAxisLayout(rawData, smoothed, out var layout))
            return CreateFixedValueAxis(title, -0.2d, 0.2d, 0.1d);

        return new ChartAxisModel
        {
            Title = title,
            Min = layout.MinValue,
            Max = layout.MaxValue,
            Step = layout.Step,
            ShowLabels = layout.ShowLabels,
            UseDisplayValueFormatter = true
        };
    }

    public static ChartAxisModel CreateFixedValueAxis(string title, double min, double max, double step) =>
        new()
        {
            Title = title,
            Min = min,
            Max = max,
            Step = step,
            ShowLabels = true,
            UseDisplayValueFormatter = true
        };

    private static int ResolveCategoryStep(int labelCount)
    {
        if (labelCount <= 1)
            return 1;

        return Math.Max(1, (int)Math.Ceiling(labelCount / (double)ChartRenderDefaults.DesiredXAxisTickCount));
    }

    private static double NormalizeValue(double value) =>
        double.IsFinite(value) ? value : 0d;

    private static string FormatTimestamp(DateTime timestamp) =>
        timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
