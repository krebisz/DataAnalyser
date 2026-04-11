using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Kernel;

public enum AlignmentMode
{
    None = 0,
    ForwardFill = 1
}

public sealed record AlignedMetricSeries(
    MetricSeriesRequest Request,
    IReadOnlyList<double> RawValues,
    IReadOnlyList<double> SmoothedValues);

public sealed record AlignedSeriesBundle(
    IReadOnlyList<DateTime> Timeline,
    IReadOnlyList<AlignedMetricSeries> Series);

public sealed class TimeSeriesAlignmentKernel
{
    public AlignedSeriesBundle Align(
        MetricLoadSnapshot snapshot,
        AlignmentMode alignmentMode = AlignmentMode.ForwardFill,
        int smoothingWindow = 1)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var timeline = snapshot.Series
            .SelectMany(series => series.RawData.Select(data => data.NormalizedTimestamp))
            .Distinct()
            .OrderBy(timestamp => timestamp)
            .ToList();

        var aligned = snapshot.Series
            .Select(series =>
            {
                var values = AlignSeries(series.RawData, timeline, alignmentMode);
                return new AlignedMetricSeries(
                    series.Request,
                    values,
                    Smooth(values, smoothingWindow));
            })
            .ToList();

        return new AlignedSeriesBundle(timeline, aligned);
    }

    private static IReadOnlyList<double> AlignSeries(
        IReadOnlyList<MetricData> source,
        IReadOnlyList<DateTime> timeline,
        AlignmentMode alignmentMode)
    {
        var valuesByTimestamp = source
            .GroupBy(data => data.NormalizedTimestamp)
            .ToDictionary(group => group.Key, group => Convert.ToDouble(group.First().Value ?? 0m));

        var result = new List<double>(timeline.Count);
        var lastValue = double.NaN;

        foreach (var timestamp in timeline)
        {
            if (valuesByTimestamp.TryGetValue(timestamp, out var value))
            {
                result.Add(value);
                lastValue = value;
                continue;
            }

            result.Add(alignmentMode == AlignmentMode.ForwardFill ? lastValue : double.NaN);
        }

        return result;
    }

    private static IReadOnlyList<double> Smooth(IReadOnlyList<double> values, int window)
    {
        if (window <= 1 || values.Count == 0)
            return values.ToList();

        var result = new double[values.Count];
        for (var index = 0; index < values.Count; index++)
        {
            var start = Math.Max(0, index - window);
            var end = Math.Min(values.Count - 1, index + window);
            double sum = 0;
            var count = 0;

            for (var pointer = start; pointer <= end; pointer++)
            {
                if (double.IsNaN(values[pointer]))
                    continue;

                sum += values[pointer];
                count++;
            }

            result[index] = count == 0 ? double.NaN : sum / count;
        }

        return result;
    }
}
