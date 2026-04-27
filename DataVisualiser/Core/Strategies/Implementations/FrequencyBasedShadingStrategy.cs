using System.Diagnostics;
using System.Windows.Media;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Core.Strategies.Implementations;

public class FrequencyBasedShadingStrategy : IIntervalShadingStrategy
{
    private readonly int _bucketCount;

    public FrequencyBasedShadingStrategy(int bucketCount)
    {
        _bucketCount = bucketCount;
    }

    public Dictionary<int, Dictionary<int, Color>> CalculateColorMap(IntervalShadingContext context)
    {
        var maxFreq = 0;

        foreach (var bucketFreqs in context.FrequenciesPerBucket.Values)
            foreach (var freq in bucketFreqs.Values)
                if (freq > maxFreq)
                    maxFreq = freq;

        if (maxFreq == 0)
            maxFreq = 1;

        Debug.WriteLine("=== Distribution: Color Mapping (Frequency-Based) ===");
        Debug.WriteLine($"Global max frequency: {maxFreq}");

        var colorMap = new Dictionary<int, Dictionary<int, Color>>();

        for (var bucketIndex = 0; bucketIndex < _bucketCount; bucketIndex++)
        {
            var bucketColourMap = new Dictionary<int, Color>();

            if (context.FrequenciesPerBucket.TryGetValue(bucketIndex, out var bucketFreqs))
                foreach (var kvp in bucketFreqs)
                {
                    var intervalIndex = kvp.Key;
                    var frequency = kvp.Value;

                    if (frequency > 0)
                    {
                        var normalizedFreq = (double)frequency / maxFreq;
                        var color = MapNormalizedValueToColor(normalizedFreq);
                        bucketColourMap[intervalIndex] = color;
                    }
                }

            colorMap[bucketIndex] = bucketColourMap;
        }

        return colorMap;
    }

    public Color? CalculateIntervalColor(IntervalShadingContext context, int bucketIndex, int intervalIndex, int intervalMaxFrequency, int globalMaxFrequency)
    {
        var frequency = 0;
        if (context.FrequenciesPerBucket.TryGetValue(bucketIndex, out var dayFreqs) && dayFreqs.TryGetValue(intervalIndex, out var freq))
            frequency = freq;

        if (frequency == 0)
            return null;

        var normalizedFreq = globalMaxFrequency > 0 ? (double)frequency / globalMaxFrequency : 0.0;
        return MapNormalizedValueToColor(normalizedFreq);
    }

    private static Color MapNormalizedValueToColor(double normalizedValue)
    {
        normalizedValue = Math.Max(0.0, Math.Min(1.0, normalizedValue));

        byte r0 = FrequencyShadingDefaults.StartR, g0 = FrequencyShadingDefaults.StartG, b0 = FrequencyShadingDefaults.StartB;
        byte r1 = FrequencyShadingDefaults.EndR, g1 = FrequencyShadingDefaults.EndG, b1 = FrequencyShadingDefaults.EndB;

        var r = (byte)Math.Round(r0 + (r1 - r0) * normalizedValue);
        var g = (byte)Math.Round(g0 + (g1 - g0) * normalizedValue);
        var b = (byte)Math.Round(b0 + (b1 - b0) * normalizedValue);

        return Color.FromRgb(r, g, b);
    }
}
