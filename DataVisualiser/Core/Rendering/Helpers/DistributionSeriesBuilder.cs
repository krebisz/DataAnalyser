using DataVisualiser.Core.Configuration.Defaults;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;

namespace DataVisualiser.Core.Rendering.Helpers;

internal static class DistributionSeriesBuilder
{
    public static void AddBaselineAndRangeSeries(CartesianChart chart, IReadOnlyList<double> mins, IReadOnlyList<double> ranges, double globalMin, string displayName, bool useFrequencyShading, int bucketCount)
    {
        var baseline = CreateBaselineSeries(displayName);
        var range = CreateRangeSeries(displayName);

        for (var i = 0; i < bucketCount; i++)
        {
            var rangeVal = double.IsNaN(ranges[i]) || ranges[i] < 0 ? 0.0 : ranges[i];
            var baselineVal = useFrequencyShading ? globalMin : double.IsNaN(mins[i]) ? 0.0 : mins[i];

            baseline.Values.Add(baselineVal);
            range.Values.Add(rangeVal);
        }

        chart.Series.Add(baseline);
        chart.Series.Add(range);
    }

    public static void AddAverageSeries(CartesianChart chart, IReadOnlyDictionary<int, List<double>> bucketValues, string displayName, int bucketCount)
    {
        var series = ChartHelper.CreateLineSeries($"{displayName} avg", 5, 2, Color.FromRgb(235, 200, 40));
        if (series == null)
            return;

        series.LineSmoothness = 0;

        for (var i = 0; i < bucketCount; i++)
        {
            var values = bucketValues.TryGetValue(i, out var bucket) ? bucket : [];
            var validValues = values.Where(v => !double.IsNaN(v)).ToList();
            series.Values.Add(validValues.Count > 0 ? validValues.Average() : double.NaN);
        }

        chart.Series.Add(series);
    }

    private static StackedColumnSeries CreateBaselineSeries(string displayName)
    {
        return new StackedColumnSeries
        {
            Title = $"{displayName} baseline",
            Values = new ChartValues<double>(),
            Fill = Brushes.Transparent,
            StrokeThickness = 0,
            MaxColumnWidth = RenderingDefaults.MaxColumnWidth
        };
    }

    private static StackedColumnSeries CreateRangeSeries(string displayName)
    {
        return new StackedColumnSeries
        {
            Title = $"{displayName} range",
            Values = new ChartValues<double>(),
            Fill = new SolidColorBrush(Color.FromRgb(173, 216, 230)),
            Stroke = new SolidColorBrush(Color.FromRgb(60, 120, 200)),
            StrokeThickness = 1,
            MaxColumnWidth = RenderingDefaults.MaxColumnWidth
        };
    }
}
