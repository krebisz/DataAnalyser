using System.Diagnostics;
using System.Windows.Media;

namespace DataVisualiser.Core.Rendering.Shading;

/// <summary>
///     Default shading strategy that uses raw frequency counts normalized globally.
///     Higher frequency = darker color (light blue to dark blue/near-black).
///     This is the original behavior before extraction.
/// </summary>
public class FrequencyBasedShadingStrategy : IIntervalShadingStrategy
{
    private int _bucketCount = 0;

    public FrequencyBasedShadingStrategy(int bucketCount)
    {
        _bucketCount = bucketCount;
    }

    /// <summary>
    ///     Calculates color map using global frequency normalization.
    ///     Finds the maximum frequency across all days and intervals, then normalizes each frequency
    ///     to [0.0, 1.0] based on this global maximum.
    /// </summary>
    public Dictionary<int, Dictionary<int, Color>> CalculateColorMap(IntervalShadingContext context)
    {
        // Find global maximum frequency across all days and intervals
        var maxFreq = 0;

        foreach (var bucketFreqs in context.FrequenciesPerBucket.Values)
            foreach (var freq in bucketFreqs.Values)
                if (freq > maxFreq)
                    maxFreq = freq;

        if (maxFreq == 0)
            maxFreq = 1; // Avoid division by zero

        Debug.WriteLine("=== Distribution: Color Mapping (Frequency-Based) ===");
        Debug.WriteLine($"Global max frequency: {maxFreq}");

        // Map each non-zero frequency to a color (light blue to dark blue/near-black)
        var colorMap = new Dictionary<int, Dictionary<int, Color>>();

        for (var bucketIndex = 0; bucketIndex < _bucketCount; bucketIndex++)
        {
            var bucketColourMap = new Dictionary<int, Color>();

            if (context.FrequenciesPerBucket.TryGetValue(bucketIndex, out var bucketFreqs))
                foreach (var kvp in bucketFreqs)
                {
                    var intervalIndex = kvp.Key;
                    var frequency = kvp.Value;

                    // Only map non-zero frequencies to colors
                    // Zero frequencies will be handled separately (white if within day's range)
                    if (frequency > 0)
                    {
                        // Normalize frequency to [0.0, 1.0] based on global maximum
                        var normalizedFreq = (double)frequency / maxFreq;

                        // Map to color (light blue = low frequency, dark blue/near-black = high frequency)
                        var color = MapNormalizedValueToColor(normalizedFreq);
                        bucketColourMap[intervalIndex] = color;
                    }
                    // Note: frequency = 0 is not added to dayColorMap here
                    // It will be handled separately based on whether interval overlaps day's range
                }

            colorMap[bucketIndex] = bucketColourMap;
        }

        return colorMap;
    }

    /// <summary>
    ///     Calculates color for a specific interval using global frequency normalization.
    ///     This ensures consistent shading across all intervals - a frequency of 1 will always be
    ///     lighter than a frequency of 10, regardless of which interval they're in.
    /// </summary>
    public Color? CalculateIntervalColor(IntervalShadingContext context, int bucketIndex, int intervalIndex, int intervalMaxFrequency, int globalMaxFrequency)
    {
        // Get frequency for this day/interval
        var frequency = 0;
        if (context.FrequenciesPerBucket.TryGetValue(bucketIndex, out var dayFreqs) && dayFreqs.TryGetValue(intervalIndex, out var freq))
            frequency = freq;

        // Zero frequency intervals should be white (handled separately)
        if (frequency == 0)
            return null;

        // Normalize using GLOBAL max frequency to ensure consistent shading across all intervals
        // This matches the behavior of CalculateColorMap which also uses global normalization
        var normalizedFreq = globalMaxFrequency > 0 ? (double)frequency / globalMaxFrequency : 0.0;

        return MapNormalizedValueToColor(normalizedFreq);
    }

    /// <summary>
    ///     Maps a normalized value [0.0, 1.0] to a color gradient from light blue to dark blue/near-black.
    /// </summary>
    private static Color MapNormalizedValueToColor(double normalizedValue)
    {
        // Clamp to [0.0, 1.0]
        normalizedValue = Math.Max(0.0, Math.Min(1.0, normalizedValue));

        // Start color: light blue (when value = 0)
        byte r0 = 173, g0 = 216, b0 = 230;

        // End color: near-black/dark blue (when value = 1.0)
        byte r1 = 8, g1 = 10, b1 = 25;

        // Interpolate based on normalized value
        var r = (byte)Math.Round(r0 + (r1 - r0) * normalizedValue);
        var g = (byte)Math.Round(g0 + (g1 - g0) * normalizedValue);
        var b = (byte)Math.Round(b0 + (b1 - b0) * normalizedValue);

        return Color.FromRgb(r, g, b);
    }
}