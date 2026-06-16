using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Computation.TimeSeries;

public static class TimeSeriesAlignment
{
    public static IReadOnlyList<DateTime> BuildUnifiedDailyTimeline(IEnumerable<MetricData> primarySeries, IEnumerable<MetricData> secondarySeries)
    {
        return primarySeries
            .Select(d => d.NormalizedTimestamp.Date)
            .Concat(secondarySeries.Select(d => d.NormalizedTimestamp.Date))
            .Distinct()
            .OrderBy(d => d)
            .ToList();
    }

    public static IReadOnlyList<double> AlignDailyForwardFilledValues(IEnumerable<MetricData> source, IReadOnlyList<DateTime> timeline, double seedValue)
    {
        var valuesByDate = source
            .GroupBy(d => d.NormalizedTimestamp.Date)
            .ToDictionary(g => g.Key, g => Convert.ToDouble(g.First().Value ?? 0m));

        var lastValue = seedValue;
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

    public static List<double> AlignSeriesToTimeline(IReadOnlyList<DateTime> seriesTimestamps, IReadOnlyList<double> seriesValues, IReadOnlyList<DateTime> mainTimeline)
    {
        if (seriesTimestamps.Count == 0 || seriesValues.Count == 0)
            return mainTimeline.Select(_ => double.NaN).ToList();

        var count = Math.Min(seriesTimestamps.Count, seriesValues.Count);
        if (count == 0)
            return mainTimeline.Select(_ => double.NaN).ToList();

        var valueMap = new Dictionary<DateTime, double>();
        for (var i = 0; i < count; i++)
            valueMap[seriesTimestamps[i]] = seriesValues[i];

        var aligned = new List<double>(mainTimeline.Count);
        var lastValue = double.NaN;

        foreach (var timestamp in mainTimeline)
            if (valueMap.TryGetValue(timestamp, out var exactValue))
            {
                aligned.Add(exactValue);
                lastValue = exactValue;
            }
            else
            {
                var day = timestamp.Date;
                var dayMatch = valueMap.Keys.FirstOrDefault(ts => ts.Date == day);

                if (dayMatch != default && valueMap.TryGetValue(dayMatch, out var dayValue))
                {
                    aligned.Add(dayValue);
                    lastValue = dayValue;
                }
                else if (!double.IsNaN(lastValue))
                {
                    aligned.Add(lastValue);
                }
                else
                {
                    aligned.Add(double.NaN);
                }
            }

        return aligned;
    }
}
