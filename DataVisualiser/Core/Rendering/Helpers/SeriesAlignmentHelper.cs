using System;
using System.Collections.Generic;
using System.Linq;

namespace DataVisualiser.Core.Rendering.Helpers;

public static class SeriesAlignmentHelper
{
    public static List<double> AlignSeriesToTimeline(List<DateTime> seriesTimestamps, List<double> seriesValues, List<DateTime> mainTimeline)
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
