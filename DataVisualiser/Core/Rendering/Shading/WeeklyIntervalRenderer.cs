using System.Diagnostics;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Shading;

/// <summary>
///     Renders interval series for weekly distribution charts.
///     Extracted from WeeklyDistributionService to improve testability and maintainability.
/// </summary>
public sealed class WeeklyIntervalRenderer
{
    private const double MaxColumnWidth = 40.0;
    private const int    BucketCount    = 7;

    /// <summary>
    ///     Renders interval series on the chart.
    /// </summary>
    public int RenderIntervals(CartesianChart chart, List<double> mins, List<double> ranges, List<(double Min, double Max)> intervals, Dictionary<int, Dictionary<int, int>> frequenciesPerBucket, Dictionary<int, Dictionary<int, Color>> colorMap, double uniformIntervalHeight, double[] cumulativeStackHeight, int globalMaxFreq)
    {
        var seriesCreated = 0;

        for (var intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            var state = BuildIntervalState(intervalIndex, intervals[intervalIndex], mins, ranges, frequenciesPerBucket, uniformIntervalHeight, cumulativeStackHeight);

            if (!state.HasData)
                continue;

            EmitBaseline(chart, state);

            seriesCreated += EmitIntervalSeries(chart, intervalIndex, state, frequenciesPerBucket, colorMap);
        }

        return seriesCreated;
    }

    private static void EmitBaseline(CartesianChart chart, IntervalRenderState state)
    {
        chart.Series.Add(new StackedColumnSeries
        {
                Title = null,
                Values = state.Baselines,
                Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                StrokeThickness = 0,
                MaxColumnWidth = MaxColumnWidth,
                DataLabels = false
        });
    }

    private int EmitIntervalSeries(CartesianChart chart, int intervalIndex, IntervalRenderState state, Dictionary<int, Dictionary<int, int>> frequenciesPerBucket, Dictionary<int, Dictionary<int, Color>> colorMap)
    {
        var created = 0;

        if (state.HasZeroFreqBuckets)
        {
            AddWhiteSeries(chart, state.WhiteHeights);
            created++;
        }

        if (state.HasNonZeroFreqBuckets)
        {
            var color = ResolveIntervalColor(frequenciesPerBucket, colorMap, intervalIndex);

            AddColoredSeries(chart, state.ColoredHeights, color);
            created++;
        }

        return created;
    }

    private IntervalRenderState BuildIntervalState(int intervalIndex, (double Min, double Max) interval, List<double> mins, List<double> ranges, Dictionary<int, Dictionary<int, int>> frequenciesPerBucket, double uniformIntervalHeight, double[] cumulativeStackHeight)
    {
        var state = new IntervalRenderState(uniformIntervalHeight);

        for (var bucketIndex = 0; bucketIndex < BucketCount; bucketIndex++)
        {
            if (!TryComputeBucketIntervalOverlap(bucketIndex, interval, mins, ranges, out var bucketMin, out var bucketMax))
            {
                state.AddEmpty();
                continue;
            }

            var frequency = ResolveFrequency(frequenciesPerBucket, bucketIndex, intervalIndex);

            var baseline = ComputeBaseline(interval.Min, uniformIntervalHeight, ref cumulativeStackHeight[bucketIndex]);

            state.Add(baseline, frequency);

            if (bucketIndex == 1)
                Debug.WriteLine($"  Interval {intervalIndex} [{interval.Min:F4}, {interval.Max:F4}] Tue: Baseline={baseline:F4}, Height={uniformIntervalHeight:F6}, Freq={frequency}");
        }

        return state;
    }

    private static bool TryComputeBucketIntervalOverlap(int bucketIndex, (double Min, double Max) interval, List<double> mins, List<double> ranges, out double bucketMin, out double bucketMax)
    {
        bucketMin = SafeValue(mins, bucketIndex);
        var bucketRange = SafePositive(ranges, bucketIndex);
        bucketMax = bucketMin + bucketRange;

        return bucketRange > 0 && interval.Min < bucketMax && interval.Max > bucketMin;
    }

    private static int ResolveFrequency(Dictionary<int, Dictionary<int, int>> frequenciesPerBucket, int bucketIndex, int intervalIndex)
    {
        return frequenciesPerBucket.TryGetValue(bucketIndex, out var bucketFreqs) && bucketFreqs.TryGetValue(intervalIndex, out var freq) ? freq : 0;
    }

    private static double ComputeBaseline(double intervalMin, double intervalHeight, ref double cumulativeStack)
    {
        var baseline = intervalMin - cumulativeStack;

        if (baseline < 0)
        {
            baseline = 0;
            cumulativeStack += intervalHeight;
        }
        else
        {
            cumulativeStack = intervalMin + intervalHeight;
        }

        return baseline;
    }

    private static double SafeValue(List<double> values, int index)
    {
        var v = values[index];
        return double.IsNaN(v) ? 0.0 : v;
    }

    private static double SafePositive(List<double> values, int index)
    {
        var v = values[index];
        return double.IsNaN(v) || v < 0 ? 0.0 : v;
    }


    private void AddBaselineSeries(CartesianChart chart, ChartValues<double> baselineValues)
    {
        chart.Series.Add(new StackedColumnSeries
        {
                Title = null,
                Values = baselineValues,
                Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                StrokeThickness = 0,
                MaxColumnWidth = MaxColumnWidth,
                DataLabels = false
        });
    }

    private void AddWhiteSeries(CartesianChart chart, ChartValues<double> whiteValues)
    {
        var whiteBrush = new SolidColorBrush(Colors.White);
        whiteBrush.Freeze();

        var strokeBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
        strokeBrush.Freeze();

        chart.Series.Add(new StackedColumnSeries
        {
                Title = null,
                Values = whiteValues,
                Fill = whiteBrush,
                Stroke = strokeBrush,
                StrokeThickness = 1.0,
                MaxColumnWidth = MaxColumnWidth,
                DataLabels = false
        });
    }

    private void AddColoredSeries(CartesianChart chart, ChartValues<double> coloredValues, Color fillColor)
    {
        var fillBrush = new SolidColorBrush(fillColor);
        fillBrush.Freeze();

        var strokeBrush = Darken(fillColor);
        strokeBrush.Freeze();

        chart.Series.Add(new StackedColumnSeries
        {
                Title = null,
                Values = coloredValues,
                Fill = fillBrush,
                Stroke = strokeBrush,
                StrokeThickness = 1.0,
                MaxColumnWidth = MaxColumnWidth,
                DataLabels = false
        });
    }

    private double SafeMin(List<double> mins, int index)
    {
        var v = mins[index];
        return double.IsNaN(v) ? 0.0 : v;
    }

    private double SafeRange(List<double> ranges, int index)
    {
        var v = ranges[index];
        return double.IsNaN(v) || v < 0 ? 0.0 : v;
    }

    private Color ResolveIntervalColor(Dictionary<int, Dictionary<int, int>> frequenciesPerBucket, Dictionary<int, Dictionary<int, Color>> colorMap, int intervalIndex)
    {
        // Pick the bucket with the highest frequency for this interval (most representative color).
        var bestBucket = -1;
        var bestFreq = 0;

        for (var bucketIndex = 0; bucketIndex < BucketCount; bucketIndex++)
            if (frequenciesPerBucket.TryGetValue(bucketIndex, out var bucketFreqs) && bucketFreqs.TryGetValue(intervalIndex, out var freq) && freq > bestFreq)
            {
                bestFreq = freq;
                bestBucket = bucketIndex;
            }

        if (bestBucket >= 0 && colorMap.TryGetValue(bestBucket, out var bucketColorMap) && bucketColorMap.TryGetValue(intervalIndex, out var color))
            return color;

        return Colors.Gray; // Fallback
    }

    private SolidColorBrush Darken(Color c)
    {
        var r = (byte)Math.Max(0, c.R - 30);
        var g = (byte)Math.Max(0, c.G - 30);
        var b = (byte)Math.Max(0, c.B - 30);
        return new SolidColorBrush(Color.FromRgb(r, g, b));
    }

    private sealed class IntervalRenderState
    {
        private readonly double _uniformIntervalHeight;

        public IntervalRenderState(double uniformIntervalHeight)
        {
            _uniformIntervalHeight = uniformIntervalHeight;
        }

        public ChartValues<double> Baselines          { get; } = new();
        public ChartValues<double> WhiteHeights       { get; } = new();
        public ChartValues<double> ColoredHeights     { get; } = new();
        public bool                HasData            { get; private set; }
        public bool                HasZeroFreqBuckets    { get; private set; }
        public bool                HasNonZeroFreqBuckets { get; private set; }

        public void AddEmpty()
        {
            Baselines.Add(0);
            WhiteHeights.Add(0);
            ColoredHeights.Add(0);
        }

        public void Add(double baseline, int frequency)
        {
            HasData = true;
            Baselines.Add(baseline);

            if (frequency == 0)
            {
                WhiteHeights.Add(_uniformIntervalHeight);
                ColoredHeights.Add(0);
                HasZeroFreqBuckets = true;
            }
            else
            {
                WhiteHeights.Add(0);
                ColoredHeights.Add(_uniformIntervalHeight);
                HasNonZeroFreqBuckets = true;
            }
        }
    }
}