using System.Windows.Media;
using DataVisualiser.Core.Configuration.Defaults;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Shading;

public sealed class FrequencyShadingRenderer : IFrequencyShadingRenderer
{
    private readonly int _bucketCount;
    private readonly double _maxColumnWidth;

    public FrequencyShadingRenderer(double maxColumnWidth, int bucketCount)
    {
        _maxColumnWidth = maxColumnWidth;
        _bucketCount = bucketCount;
    }

    public void Render(CartesianChart targetChart, List<double> mins, List<double> ranges, List<(double Min, double Max)> intervals, Dictionary<int, Dictionary<int, int>> frequenciesPerBucket, Dictionary<int, Dictionary<int, Color>> colorMap, double globalMin, double globalMax, IntervalShadingContext shadingContext)
    {
        RemoveExistingRangeSeries(targetChart);

        if (!CanApplyFrequencyShading(intervals, frequenciesPerBucket, mins, ranges))
        {
            RestoreSimpleRangeSeries(targetChart, ranges);
            return;
        }

        var uniformIntervalHeight = CalculateUniformIntervalHeight(globalMin, globalMax, intervals.Count);

        var globalMaxFreq = CalculateGlobalMaxFrequency(frequenciesPerBucket);

        var cumulativeStackHeight = InitializeCumulativeStack(globalMin, _bucketCount);

        var seriesCreated = RenderIntervals(targetChart, mins, ranges, intervals, frequenciesPerBucket, colorMap, uniformIntervalHeight, cumulativeStackHeight, globalMaxFreq);

        if (seriesCreated == 0)
            RestoreSimpleRangeSeries(targetChart, ranges);
    }

    #region Interval state

    private IntervalRenderState BuildIntervalState(int intervalIndex, (double Min, double Max) interval, List<double> mins, List<double> ranges, Dictionary<int, Dictionary<int, int>> frequenciesPerBucket, double uniformIntervalHeight, double[] cumulativeStackHeight)
    {
        var state = new IntervalRenderState(uniformIntervalHeight);

        for (var bucketIndex = 0; bucketIndex < _bucketCount; bucketIndex++)
        {
            var bucketMin = SafeMin(mins, bucketIndex);
            var bucketRange = SafeRange(ranges, bucketIndex);
            var bucketMax = bucketMin + bucketRange;

            var overlaps = bucketRange > 0 && interval.Min < bucketMax && interval.Max > bucketMin;

            if (!overlaps)
            {
                state.AddEmpty();
                continue;
            }

            var frequency = frequenciesPerBucket.TryGetValue(bucketIndex, out var df) && df.TryGetValue(intervalIndex, out var f) ? f : 0;

            var desiredPosition = interval.Min;
            var currentStack = cumulativeStackHeight[bucketIndex];
            var baseline = desiredPosition - currentStack;

            if (baseline < 0)
            {
                baseline = 0;
                cumulativeStackHeight[bucketIndex] = currentStack + uniformIntervalHeight;
            }
            else
            {
                cumulativeStackHeight[bucketIndex] = interval.Min + uniformIntervalHeight;
            }

            state.Add(baseline, frequency);
        }

        return state;
    }

    #endregion

    #region IntervalRenderState (private, fully encapsulated)

    private sealed class IntervalRenderState
    {
        private readonly double _height;

        public IntervalRenderState(double height)
        {
            _height = height;
        }

        public ChartValues<double> Baselines { get; } = new();
        public ChartValues<double> WhiteHeights { get; } = new();
        public ChartValues<double> ColoredHeights { get; } = new();

        public bool HasData { get; private set; }
        public bool HasZeroFreqBuckets { get; private set; }
        public bool HasNonZeroFreqBuckets { get; private set; }

        public void Add(double baseline, int frequency)
        {
            HasData = true;
            Baselines.Add(baseline);

            if (frequency == 0)
            {
                WhiteHeights.Add(_height);
                ColoredHeights.Add(0);
                HasZeroFreqBuckets = true;
            }
            else
            {
                WhiteHeights.Add(0);
                ColoredHeights.Add(_height);
                HasNonZeroFreqBuckets = true;
            }
        }

        public void AddEmpty()
        {
            Baselines.Add(0);
            WhiteHeights.Add(0);
            ColoredHeights.Add(0);
        }
    }

    #endregion


    #region Core rendering

    private int RenderIntervals(CartesianChart chart, List<double> mins, List<double> ranges, List<(double Min, double Max)> intervals, Dictionary<int, Dictionary<int, int>> frequenciesPerBucket, Dictionary<int, Dictionary<int, Color>> colorMap, double uniformIntervalHeight, double[] cumulativeStackHeight, int globalMaxFreq)
    {
        var seriesCreated = 0;

        for (var intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            var intervalState = BuildIntervalState(intervalIndex, intervals[intervalIndex], mins, ranges, frequenciesPerBucket, uniformIntervalHeight, cumulativeStackHeight);

            if (!intervalState.HasData)
                continue;

            AddBaselineSeries(chart, intervalState.Baselines);

            if (intervalState.HasZeroFreqBuckets)
            {
                AddWhiteSeries(chart, intervalState.WhiteHeights);
                seriesCreated++;
            }

            if (intervalState.HasNonZeroFreqBuckets)
            {
                var color = ResolveIntervalColor(frequenciesPerBucket, colorMap, intervalIndex, _bucketCount);
                AddColoredSeries(chart, intervalState.ColoredHeights, color);
                seriesCreated++;
            }
        }

        return seriesCreated;
    }

    private Color ResolveIntervalColor(Dictionary<int, Dictionary<int, int>> frequenciesPerBucket, Dictionary<int, Dictionary<int, Color>> colorMap, int intervalIndex, int bucketCount)
    {
        var (bestBucket, bestFreq) = FindMostRepresentativeBucket(frequenciesPerBucket, intervalIndex, bucketCount);

        if (bestBucket >= 0 && colorMap.TryGetValue(bestBucket, out var bucketColourMap) && bucketColourMap.TryGetValue(intervalIndex, out var chosen))
            return chosen;

        return FrequencyShadingDefaults.FallbackColor; // fallback
    }

    private static(int Bucket, int Frequency) FindMostRepresentativeBucket(Dictionary<int, Dictionary<int, int>> frequenciesPerBucket, int intervalIndex, int bucketCount)
    {
        var bestBucket = -1;
        var bestFreq = 0;

        for (var bucketIndex = 0; bucketIndex < bucketCount; bucketIndex++)
            if (frequenciesPerBucket.TryGetValue(bucketIndex, out var bucketFreqs) && bucketFreqs.TryGetValue(intervalIndex, out var freq) && freq > bestFreq)
            {
                bestFreq = freq;
                bestBucket = bucketIndex;
            }

        return (bestBucket, bestFreq);
    }

    #endregion

    #region Series creation

    private void AddBaselineSeries(CartesianChart chart, ChartValues<double> values)
    {
        chart.Series.Add(new StackedColumnSeries
        {
                Values = values,
                Fill = Brushes.Transparent,
                StrokeThickness = 0,
                MaxColumnWidth = _maxColumnWidth
        });
    }

    private void AddWhiteSeries(CartesianChart chart, ChartValues<double> values)
    {
        chart.Series.Add(new StackedColumnSeries
        {
                Values = values,
                Fill = Brushes.White,
                Stroke = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                StrokeThickness = 1,
                MaxColumnWidth = _maxColumnWidth
        });
    }

    private void AddColoredSeries(CartesianChart chart, ChartValues<double> values, Color color)
    {
        chart.Series.Add(new StackedColumnSeries
        {
                Values = values,
                Fill = new SolidColorBrush(color),
                Stroke = Darken(color),
                StrokeThickness = 1,
                MaxColumnWidth = _maxColumnWidth
        });
    }

    #endregion

    #region Utilities & guards

    private void RemoveExistingRangeSeries(CartesianChart chart)
    {
        var toRemove = chart.Series.Where(s => s.Title?.Contains("range") == true).ToList();
        foreach (var s in toRemove)
            chart.Series.Remove(s);
    }

    private bool CanApplyFrequencyShading(List<(double Min, double Max)> intervals, Dictionary<int, Dictionary<int, int>> frequencies, List<double> mins, List<double> ranges)
    {
        if (intervals.Count == 0 || frequencies.Count == 0)
            return false;

        for (var bucket = 0; bucket < _bucketCount; bucket++)
        {
            var bucketMin = SafeMin(mins, bucket);
            var bucketMax = bucketMin + SafeRange(ranges, bucket);

            foreach (var iv in intervals)
                if (iv.Min < bucketMax && iv.Max > bucketMin && ranges[bucket] > 0)
                    return true;
        }

        return false;
    }

    private void RestoreSimpleRangeSeries(CartesianChart chart, List<double> ranges)
    {
        var series = new StackedColumnSeries
        {
                Title = "range",
                Values = new ChartValues<double>(),
                Fill = new SolidColorBrush(FrequencyShadingDefaults.FallbackColor),
                Stroke = new SolidColorBrush(Color.FromRgb(60, 120, 200)),
                StrokeThickness = 1,
                MaxColumnWidth = _maxColumnWidth
        };

        for (var i = 0; i < _bucketCount; i++)
            series.Values.Add(SafeRange(ranges, i));

        chart.Series.Add(series);
    }

    private static double[] InitializeCumulativeStack(double globalMin, int bucketCount)
    {
        var arr = new double[bucketCount];
        Array.Fill(arr, globalMin);
        return arr;
    }

    private static double CalculateUniformIntervalHeight(double min, double max, int count)
    {
        return count > 0 ? (max - min) / count : 1.0;
    }

    private static int CalculateGlobalMaxFrequency(Dictionary<int, Dictionary<int, int>> freq)
    {
        return freq.Values.SelectMany(v => v.Values).DefaultIfEmpty(1).Max();
    }

    private static double SafeMin(List<double> mins, int i)
    {
        return double.IsNaN(mins[i]) ? 0.0 : mins[i];
    }

    private static double SafeRange(List<double> ranges, int i)
    {
        return double.IsNaN(ranges[i]) || ranges[i] < 0 ? 0.0 : ranges[i];
    }

    private static SolidColorBrush Darken(Color c)
    {
        return new SolidColorBrush(Color.FromRgb((byte)(c.R * 0.7), (byte)(c.G * 0.7), (byte)(c.B * 0.7)));
    }

    #endregion
}