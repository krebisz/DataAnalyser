using System.Diagnostics;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Services.WeeklyDistribution;

/// <summary>
///     Renders interval series for weekly distribution charts.
///     Extracted from WeeklyDistributionService to improve testability and maintainability.
/// </summary>
public sealed class WeeklyIntervalRenderer
{
    private const double MaxColumnWidth = 40.0;

    /// <summary>
    ///     Renders interval series on the chart.
    /// </summary>
    public int RenderIntervals(CartesianChart chart, List<double> mins, List<double> ranges, List<(double Min, double Max)> intervals, Dictionary<int, Dictionary<int, int>> frequenciesPerDay, Dictionary<int, Dictionary<int, Color>> colorMap, double uniformIntervalHeight, double[] cumulativeStackHeight, int globalMaxFreq)
    {
        var seriesCreated = 0;

        for (var intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            var interval = intervals[intervalIndex];

            var state = BuildIntervalState(intervalIndex, interval, mins, ranges, frequenciesPerDay, uniformIntervalHeight, cumulativeStackHeight);

            if (!state.HasData)
                continue;

            // 1) Baseline series (transparent) to position this interval at its absolute Y value.
            AddBaselineSeries(chart, state.Baselines);

            // 2) White fill for zero-frequency segments inside the day's value range.
            if (state.HasZeroFreqDays)
            {
                AddWhiteSeries(chart, state.WhiteHeights);
                seriesCreated++;
            }

            // 3) Colored fill for non-zero frequency segments.
            if (state.HasNonZeroFreqDays)
            {
                var color = ResolveIntervalColor(frequenciesPerDay, colorMap, intervalIndex);
                AddColoredSeries(chart, state.ColoredHeights, color);
                seriesCreated++;
            }
        }

        return seriesCreated;
    }

    private IntervalRenderState BuildIntervalState(int intervalIndex, (double Min, double Max) interval, List<double> mins, List<double> ranges, Dictionary<int, Dictionary<int, int>> frequenciesPerDay, double uniformIntervalHeight, double[] cumulativeStackHeight)
    {
        var state = new IntervalRenderState(uniformIntervalHeight);

        for (var dayIndex = 0; dayIndex < 7; dayIndex++)
        {
            var dayMin = SafeMin(mins, dayIndex);
            var dayRange = SafeRange(ranges, dayIndex);
            var dayMax = dayMin + dayRange;

            var hasDayData = dayRange > 0 && !double.IsNaN(ranges[dayIndex]);
            var intervalOverlapsDayRange = interval.Min < dayMax && interval.Max > dayMin;

            if (!hasDayData || !intervalOverlapsDayRange)
            {
                state.AddEmpty();
                continue;
            }

            // Frequency lookup for this day/interval.
            var frequency = 0;
            if (frequenciesPerDay.TryGetValue(dayIndex, out var dayFreqs) && dayFreqs.TryGetValue(intervalIndex, out var f))
                frequency = f;

            // Baseline positioning:
            // We want the *top of the current stack* to move to interval.Min, then add intervalHeight.
            // Since series stack, we store "baseline = desiredPosition - currentStackHeight".
            var desiredPosition = interval.Min;
            var currentStack = cumulativeStackHeight[dayIndex];
            var baseline = desiredPosition - currentStack;

            if (baseline < 0)
            {
                // If already past desired position, clamp baseline and just stack forward.
                baseline = 0;
                cumulativeStackHeight[dayIndex] = currentStack + uniformIntervalHeight;
            }
            else
            {
                cumulativeStackHeight[dayIndex] = interval.Min + uniformIntervalHeight;
            }

            state.Add(baseline, frequency);

            // Optional targeted debug (kept compact).
            if (dayIndex == 1) // Tuesday
                Debug.WriteLine($"  Interval {intervalIndex} [{interval.Min:F4}, {interval.Max:F4}] Tue: Baseline={baseline:F4}, Height={uniformIntervalHeight:F6}, Freq={frequency}");
        }

        return state;
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

    private Color ResolveIntervalColor(Dictionary<int, Dictionary<int, int>> frequenciesPerDay, Dictionary<int, Dictionary<int, Color>> colorMap, int intervalIndex)
    {
        // Pick the day with the highest frequency for this interval (most representative color).
        var bestDay = -1;
        var bestFreq = 0;

        for (var dayIndex = 0; dayIndex < 7; dayIndex++)
            if (frequenciesPerDay.TryGetValue(dayIndex, out var dayFreqs) && dayFreqs.TryGetValue(intervalIndex, out var freq) && freq > bestFreq)
            {
                bestFreq = freq;
                bestDay = dayIndex;
            }

        if (bestDay >= 0 && colorMap.TryGetValue(bestDay, out var dayColorMap) && dayColorMap.TryGetValue(intervalIndex, out var color))
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

        public ChartValues<double> Baselines { get; } = new();
        public ChartValues<double> WhiteHeights { get; } = new();
        public ChartValues<double> ColoredHeights { get; } = new();
        public bool HasData { get; private set; }
        public bool HasZeroFreqDays { get; private set; }
        public bool HasNonZeroFreqDays { get; private set; }

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
                HasZeroFreqDays = true;
            }
            else
            {
                WhiteHeights.Add(0);
                ColoredHeights.Add(_uniformIntervalHeight);
                HasNonZeroFreqDays = true;
            }
        }
    }
}