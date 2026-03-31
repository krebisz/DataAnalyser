using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Orchestration.Builders;

internal static class ChartDataSeriesPreparationHelper
{
    internal static (
        IReadOnlyList<DateTime> Timestamps,
        IReadOnlyList<double> RawValues1,
        IReadOnlyList<double> RawValues2,
        IReadOnlyList<double> SmoothedValues1,
        IReadOnlyList<double> SmoothedValues2,
        IReadOnlyList<double> DifferenceValues,
        IReadOnlyList<double> RatioValues,
        IReadOnlyList<double> NormalizedValues1,
        IReadOnlyList<double> NormalizedValues2)
        Prepare(List<MetricData> primarySeries, List<MetricData> secondarySeries, int smoothingWindow)
    {
        var timestamps = BuildUnifiedTimeline(primarySeries, secondarySeries);
        var rawValues1 = AlignValues(primarySeries, timestamps);
        var rawValues2 = AlignValues(secondarySeries, timestamps);

        return (
            timestamps,
            rawValues1,
            rawValues2,
            Smooth(rawValues1, smoothingWindow),
            Smooth(rawValues2, smoothingWindow),
            ComputeDifference(rawValues1, rawValues2),
            ComputeRatio(rawValues1, rawValues2),
            Normalize(rawValues1),
            Normalize(rawValues2));
    }

    private static IReadOnlyList<DateTime> BuildUnifiedTimeline(List<MetricData> primarySeries, List<MetricData> secondarySeries)
    {
        return primarySeries
            .Select(d => d.NormalizedTimestamp.Date)
            .Concat(secondarySeries.Select(d => d.NormalizedTimestamp.Date))
            .Distinct()
            .OrderBy(d => d)
            .ToList();
    }

    private static IReadOnlyList<double> AlignValues(List<MetricData> source, IReadOnlyList<DateTime> timeline)
    {
        var valuesByDate = source
            .GroupBy(d => d.NormalizedTimestamp.Date)
            .ToDictionary(g => g.Key, g => Convert.ToDouble(g.First().Value ?? 0m));

        var lastValue = ComputationDefaults.ForwardFillSeedValue;
        var aligned = new List<double>(timeline.Count);

        foreach (var day in timeline)
        {
            if (valuesByDate.TryGetValue(day, out var value))
            {
                aligned.Add(value);
                lastValue = value;
                continue;
            }

            aligned.Add(lastValue);
        }

        return aligned;
    }

    private static IReadOnlyList<double> Smooth(IReadOnlyList<double> values, int window)
    {
        if (window <= 1)
            return values;

        var result = new double[values.Count];

        for (var i = 0; i < values.Count; i++)
        {
            var start = Math.Max(0, i - window);
            var end = Math.Min(values.Count - 1, i + window);
            var count = end - start + 1;

            double sum = 0;
            for (var j = start; j <= end; j++)
                sum += values[j];

            result[i] = sum / count;
        }

        return result.ToList();
    }

    private static IReadOnlyList<double> ComputeDifference(IReadOnlyList<double> primaryValues, IReadOnlyList<double> secondaryValues)
    {
        var result = new double[primaryValues.Count];
        for (var i = 0; i < primaryValues.Count; i++)
            result[i] = primaryValues[i] - secondaryValues[i];

        return result.ToList();
    }

    private static IReadOnlyList<double> ComputeRatio(IReadOnlyList<double> primaryValues, IReadOnlyList<double> secondaryValues)
    {
        var result = new double[primaryValues.Count];
        for (var i = 0; i < primaryValues.Count; i++)
            result[i] = secondaryValues[i] == 0 ? ComputationDefaults.RatioDivideByZeroValue : primaryValues[i] / secondaryValues[i];

        return result.ToList();
    }

    private static IReadOnlyList<double> Normalize(IReadOnlyList<double> values)
    {
        var max = values.Max();
        if (max <= 0)
            return values.ToList();

        return values.Select(v => v / max).ToList();
    }
}
