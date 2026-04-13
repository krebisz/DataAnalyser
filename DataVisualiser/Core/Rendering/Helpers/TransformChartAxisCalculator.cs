using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Rendering.Helpers;

public sealed class TransformChartAxisLayout
{
    public double MinValue { get; init; }

    public double MaxValue { get; init; }

    public double? Step { get; init; }

    public bool ShowLabels { get; init; }
}

public static class TransformChartAxisCalculator
{
    public static bool TryCreateYAxisLayout(List<MetricData> rawData, List<double> smoothedValues, out TransformChartAxisLayout layout)
    {
        var allValues = CollectAllValues(rawData, smoothedValues);
        if (!TryGetValidMinMax(allValues, out var dataMin, out var dataMax))
        {
            layout = new TransformChartAxisLayout
            {
                    MinValue = double.NaN,
                    MaxValue = double.NaN,
                    Step = null,
                    ShowLabels = false
            };
            return false;
        }

        var padded = CalculatePaddedRange(dataMin, dataMax);
        var step = padded.Range / 10.0;

        if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step))
        {
            layout = new TransformChartAxisLayout
            {
                    MinValue = MathHelper.RoundToThreeSignificantDigits(padded.Min),
                    MaxValue = MathHelper.RoundToThreeSignificantDigits(padded.Max),
                    Step = null,
                    ShowLabels = true
            };
            return true;
        }

        var niceInterval = CalculateNiceInterval(padded.Range);
        var (niceMin, niceMax) = CalculateNiceBounds(padded.Min, padded.Max, niceInterval, dataMin);
        step = MathHelper.RoundToThreeSignificantDigits(niceInterval);

        if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step))
            step = MathHelper.RoundToThreeSignificantDigits((niceMax - niceMin) / 10.0);

        layout = new TransformChartAxisLayout
        {
                MinValue = MathHelper.RoundToThreeSignificantDigits(niceMin),
                MaxValue = MathHelper.RoundToThreeSignificantDigits(niceMax),
                Step = step > 0 && !double.IsNaN(step) && !double.IsInfinity(step) ? step : null,
                ShowLabels = true
        };
        return true;
    }

    public static double CalculateChartHeight(double minValue, double maxValue, double? step, double minHeight)
    {
        if (double.IsNaN(minValue) || double.IsNaN(maxValue) || double.IsInfinity(minValue) || double.IsInfinity(maxValue))
            return minHeight;

        var resolvedStep = step ?? 0;
        if (resolvedStep <= 0 || double.IsNaN(resolvedStep) || double.IsInfinity(resolvedStep))
        {
            resolvedStep = (maxValue - minValue) / 10.0;
            if (resolvedStep <= 0 || double.IsNaN(resolvedStep) || double.IsInfinity(resolvedStep))
                return minHeight;
        }

        var range = maxValue - minValue;
        var tickCount = (int)Math.Ceiling(range / resolvedStep) + 1;
        tickCount = Math.Max(2, tickCount);

        var calculatedHeight = tickCount * RenderingDefaults.ChartTickSpacingPx + RenderingDefaults.ChartPaddingPx;
        calculatedHeight = Math.Max(minHeight, calculatedHeight);
        return Math.Min(RenderingDefaults.ChartMaxHeightPx, calculatedHeight);
    }

    private static List<double> CollectAllValues(List<MetricData> rawData, List<double> smoothedValues)
    {
        var allValues = new List<double>();

        foreach (var point in rawData)
            if (point.Value.HasValue)
                allValues.Add((double)point.Value.Value);

        foreach (var value in smoothedValues)
            if (!double.IsNaN(value) && !double.IsInfinity(value))
                allValues.Add(value);

        return allValues;
    }

    private static bool TryGetValidMinMax(List<double> values, out double min, out double max)
    {
        min = max = 0;
        if (!values.Any())
            return false;

        min = values.Min();
        max = values.Max();

        return !double.IsNaN(min) && !double.IsNaN(max) && !double.IsInfinity(min) && !double.IsInfinity(max);
    }

    private static (double Min, double Max, double Range) CalculatePaddedRange(double dataMin, double dataMax)
    {
        var range = dataMax - dataMin;
        var (minValue, maxValue) = ApplyPadding(dataMin, dataMax, range);
        return (minValue, maxValue, maxValue - minValue);
    }

    private static (double MinValue, double MaxValue) ApplyPadding(double dataMin, double dataMax, double range)
    {
        var minValue = dataMin;
        var maxValue = dataMax;

        if (range <= double.Epsilon)
        {
            var padding = Math.Max(Math.Abs(minValue) * 0.1, 1e-3);
            if (Math.Abs(minValue) < 1e-6)
            {
                minValue = -padding;
                maxValue = padding;
            }
            else
            {
                minValue -= padding;
                maxValue += padding;
                if (dataMin >= 0)
                    minValue = Math.Max(0, minValue);
            }
        }
        else
        {
            var padding = range * 0.05;
            minValue -= padding;
            maxValue += padding;
            if (dataMin >= 0)
                minValue = Math.Max(0, minValue);
        }

        return (minValue, maxValue);
    }

    private static double CalculateNiceInterval(double range, double targetTicks = 10.0)
    {
        var rawTickInterval = range / targetTicks;
        if (rawTickInterval <= 0 || double.IsNaN(rawTickInterval) || double.IsInfinity(rawTickInterval))
            return rawTickInterval;

        double magnitude;
        try
        {
            var logValue = Math.Log10(Math.Abs(rawTickInterval));
            magnitude = Math.Pow(10, Math.Floor(logValue));
        }
        catch
        {
            magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(1e-6, rawTickInterval))));
        }

        var normalizedInterval = rawTickInterval / magnitude;
        var niceInterval = normalizedInterval switch
        {
                <= 1 => 1 * magnitude,
                <= 2 => 2 * magnitude,
                <= 5 => 5 * magnitude,
                _ => 10 * magnitude
        };

        return MathHelper.RoundToThreeSignificantDigits(niceInterval);
    }

    private static (double NiceMin, double NiceMax) CalculateNiceBounds(double minValue, double maxValue, double niceInterval, double dataMin)
    {
        var niceMin = Math.Floor(minValue / niceInterval) * niceInterval;
        var niceMax = Math.Ceiling(maxValue / niceInterval) * niceInterval;

        if (dataMin >= 0 && niceMin < 0)
            niceMin = 0;

        return (niceMin, niceMax);
    }
}
