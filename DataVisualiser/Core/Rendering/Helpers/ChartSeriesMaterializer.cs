using System.Diagnostics;
using System.Windows.Media;
using DataVisualiser.Core.Computation.Results;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Helpers;

internal static class ChartSeriesMaterializer
{
    internal static LineSeries CreateAndPopulateSeries(string title, int pointSize, int lineThickness, Color color, IList<double> values, bool isStacked)
    {
        var series = CreateSeries(title, pointSize, lineThickness, color, isStacked);

        var validCount = 0;
        var nanCount = 0;
        double? firstValue = null;
        double? lastValue = null;

        foreach (var value in values)
        {
            var normalized = isStacked && (double.IsNaN(value) || double.IsInfinity(value)) ? 0.0 : value;
            series.Values.Add(normalized);
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                nanCount++;
            }
            else
            {
                validCount++;
                if (firstValue == null)
                    firstValue = normalized;
                lastValue = normalized;
            }
        }

        Debug.WriteLine($"[TransformChart] CreateAndPopulateSeries: title={title}, total={values.Count}, valid={validCount}, NaN={nanCount}, first={firstValue}, last={lastValue}");

        return series;
    }

    internal static IList<double> ResolveStackedSeriesValues(SeriesResult seriesResult, List<DateTime> mainTimeline, out bool usedSmoothed)
    {
        usedSmoothed = false;

        if (seriesResult.Smoothed != null && seriesResult.Smoothed.Count > 0)
        {
            var alignedSmoothed = SeriesAlignmentHelper.AlignSeriesToTimeline(seriesResult.Timestamps, seriesResult.Smoothed, mainTimeline);
            if (HasAnyValidValue(alignedSmoothed))
            {
                usedSmoothed = true;
                return alignedSmoothed;
            }
        }

        return SeriesAlignmentHelper.AlignSeriesToTimeline(seriesResult.Timestamps, seriesResult.RawValues, mainTimeline);
    }

    internal static bool HasAnyValidValue(IList<double> values)
    {
        return values.Any(value => !double.IsNaN(value) && !double.IsInfinity(value));
    }

    internal static (int Valid, int NaN) GetValueStats(IList<double> values)
    {
        var valid = 0;
        var nan = 0;

        foreach (var value in values)
            if (double.IsNaN(value) || double.IsInfinity(value))
                nan++;
            else
                valid++;

        return (valid, nan);
    }

    private static LineSeries CreateSeries(string title, int pointSize, int lineThickness, Color color, bool isStacked)
    {
        var series = isStacked ? new StackedAreaSeries() : new LineSeries();

        series.Title = title;
        series.Values = new ChartValues<double>();
        series.PointGeometrySize = pointSize;
        series.StrokeThickness = lineThickness;
        series.Stroke = new SolidColorBrush(color);
        series.DataLabels = false;
        series.Fill = isStacked ? new SolidColorBrush(Color.FromArgb(110, color.R, color.G, color.B)) : Brushes.Transparent;
        series.LineSmoothness = 0;

        if (series is StackedAreaSeries stackedArea)
            stackedArea.StackMode = StackMode.Values;

        return series;
    }
}
