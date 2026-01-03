using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;

namespace DataVisualiser.Core.Rendering.Shading;

public sealed class FrequencyShadingRenderer : IFrequencyShadingRenderer
{
    private readonly double _maxColumnWidth;

    public FrequencyShadingRenderer(double maxColumnWidth)
    {
        _maxColumnWidth = maxColumnWidth;
    }

    public void Render(CartesianChart targetChart, List<double> mins, List<double> ranges, List<(double Min, double Max)> intervals, Dictionary<int, Dictionary<int, int>> frequenciesPerDay, Dictionary<int, Dictionary<int, Color>> colorMap, double globalMin, double globalMax, IntervalShadingContext shadingContext)
    {
        RemoveExistingRangeSeries(targetChart);

        if (!CanApplyFrequencyShading(intervals, frequenciesPerDay, mins, ranges))
        {
            RestoreSimpleRangeSeries(targetChart, ranges);
            return;
        }

        var uniformIntervalHeight = CalculateUniformIntervalHeight(globalMin, globalMax, intervals.Count);

        var globalMaxFreq = CalculateGlobalMaxFrequency(frequenciesPerDay);

        var cumulativeStackHeight = InitializeCumulativeStack(globalMin);

        var seriesCreated = RenderIntervals(targetChart, mins, ranges, intervals, frequenciesPerDay, colorMap, uniformIntervalHeight, cumulativeStackHeight, globalMaxFreq);

        if (seriesCreated == 0)
            RestoreSimpleRangeSeries(targetChart, ranges);
    }

    #region Interval state

    private IntervalRenderState BuildIntervalState(int intervalIndex, (double Min, double Max) interval, List<double> mins, List<double> ranges, Dictionary<int, Dictionary<int, int>> frequenciesPerDay, double uniformIntervalHeight, double[] cumulativeStackHeight)
    {
        var state = new IntervalRenderState(uniformIntervalHeight);

        for (var dayIndex = 0; dayIndex < 7; dayIndex++)
        {
            var dayMin = SafeMin(mins, dayIndex);
            var dayRange = SafeRange(ranges, dayIndex);
            var dayMax = dayMin + dayRange;

            var overlaps = dayRange > 0 && interval.Min < dayMax && interval.Max > dayMin;

            if (!overlaps)
            {
                state.AddEmpty();
                continue;
            }

            var frequency = frequenciesPerDay.TryGetValue(dayIndex, out var df) && df.TryGetValue(intervalIndex, out var f) ? f : 0;

            var desiredPosition = interval.Min;
            var currentStack = cumulativeStackHeight[dayIndex];
            var baseline = desiredPosition - currentStack;

            if (baseline < 0)
            {
                baseline = 0;
                cumulativeStackHeight[dayIndex] = currentStack + uniformIntervalHeight;
            }
            else
            {
                cumulativeStackHeight[dayIndex] = interval.Min + uniformIntervalHeight;
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
        public bool HasZeroFreqDays { get; private set; }
        public bool HasNonZeroFreqDays { get; private set; }

        public void Add(double baseline, int frequency)
        {
            HasData = true;
            Baselines.Add(baseline);

            if (frequency == 0)
            {
                WhiteHeights.Add(_height);
                ColoredHeights.Add(0);
                HasZeroFreqDays = true;
            }
            else
            {
                WhiteHeights.Add(0);
                ColoredHeights.Add(_height);
                HasNonZeroFreqDays = true;
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

    private int RenderIntervals(CartesianChart chart, List<double> mins, List<double> ranges, List<(double Min, double Max)> intervals, Dictionary<int, Dictionary<int, int>> frequenciesPerDay, Dictionary<int, Dictionary<int, Color>> colorMap, double uniformIntervalHeight, double[] cumulativeStackHeight, int globalMaxFreq)
    {
        var seriesCreated = 0;

        for (var intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            var intervalState = BuildIntervalState(intervalIndex, intervals[intervalIndex], mins, ranges, frequenciesPerDay, uniformIntervalHeight, cumulativeStackHeight);

            if (!intervalState.HasData)
                continue;

            AddBaselineSeries(chart, intervalState.Baselines);

            if (intervalState.HasZeroFreqDays)
            {
                AddWhiteSeries(chart, intervalState.WhiteHeights);
                seriesCreated++;
            }

            if (intervalState.HasNonZeroFreqDays)
            {
                var color = ResolveIntervalColor(frequenciesPerDay, colorMap, intervalIndex);
                AddColoredSeries(chart, intervalState.ColoredHeights, color);
                seriesCreated++;
            }
        }

        return seriesCreated;
    }

    private Color ResolveIntervalColor(Dictionary<int, Dictionary<int, int>> frequenciesPerDay, Dictionary<int, Dictionary<int, Color>> colorMap, int intervalIndex)
    {
        var (bestDay, bestFreq) = FindMostRepresentativeDay(frequenciesPerDay, intervalIndex);

        if (bestDay >= 0 && colorMap.TryGetValue(bestDay, out var dayColorMap) && dayColorMap.TryGetValue(intervalIndex, out var chosen))
            return chosen;

        return Color.FromRgb(173, 216, 230); // fallback
    }

    private static (int Day, int Frequency) FindMostRepresentativeDay(Dictionary<int, Dictionary<int, int>> frequenciesPerDay, int intervalIndex)
    {
        var bestDay = -1;
        var bestFreq = 0;

        for (var dayIndex = 0; dayIndex < 7; dayIndex++)
            if (frequenciesPerDay.TryGetValue(dayIndex, out var dayFreqs) && dayFreqs.TryGetValue(intervalIndex, out var freq) && freq > bestFreq)
            {
                bestFreq = freq;
                bestDay = dayIndex;
            }

        return (bestDay, bestFreq);
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
        var toRemove = chart.Series.Where(s => s.Title?.Contains("range") == true).
            ToList();
        foreach (var s in toRemove)
            chart.Series.Remove(s);
    }

    private bool CanApplyFrequencyShading(List<(double Min, double Max)> intervals, Dictionary<int, Dictionary<int, int>> frequencies, List<double> mins, List<double> ranges)
    {
        if (intervals.Count == 0 || frequencies.Count == 0)
            return false;

        for (var day = 0; day < 7; day++)
        {
            var dayMin = SafeMin(mins, day);
            var dayMax = dayMin + SafeRange(ranges, day);

            foreach (var iv in intervals)
                if (iv.Min < dayMax && iv.Max > dayMin && ranges[day] > 0)
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
            Fill = new SolidColorBrush(Color.FromRgb(173, 216, 230)),
            Stroke = new SolidColorBrush(Color.FromRgb(60, 120, 200)),
            StrokeThickness = 1,
            MaxColumnWidth = _maxColumnWidth
        };

        for (var i = 0; i < 7; i++)
            series.Values.Add(SafeRange(ranges, i));

        chart.Series.Add(series);
    }

    private static double[] InitializeCumulativeStack(double globalMin)
    {
        var arr = new double[7];
        Array.Fill(arr, globalMin);
        return arr;
    }

    private static double CalculateUniformIntervalHeight(double min, double max, int count)
    {
        return count > 0 ? (max - min) / count : 1.0;
    }

    private static int CalculateGlobalMaxFrequency(Dictionary<int, Dictionary<int, int>> freq)
    {
        return freq.Values.SelectMany(v => v.Values).
            DefaultIfEmpty(1).
            Max();
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
