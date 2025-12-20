using System.Windows.Media;

namespace DataVisualiser.Services.Shading
{
    /// <summary>
    /// Default shading strategy that uses raw frequency counts normalized globally.
    /// Higher frequency = darker color (light blue to dark blue/near-black).
    /// This is the original behavior before extraction.
    /// </summary>
    public class FrequencyBasedShadingStrategy : IIntervalShadingStrategy
    {
        /// <summary>
        /// Calculates color map using global frequency normalization.
        /// Finds the maximum frequency across all days and intervals, then normalizes each frequency
        /// to [0.0, 1.0] based on this global maximum.
        /// </summary>
        public Dictionary<int, Dictionary<int, Color>> CalculateColorMap(IntervalShadingContext context)
        {
            // Find global maximum frequency across all days and intervals
            int maxFreq = 0;
            foreach (var dayFreqs in context.FrequenciesPerDay.Values)
            {
                foreach (var freq in dayFreqs.Values)
                {
                    if (freq > maxFreq)
                        maxFreq = freq;
                }
            }

            if (maxFreq == 0)
                maxFreq = 1; // Avoid division by zero

            System.Diagnostics.Debug.WriteLine($"=== WeeklyDistribution: Color Mapping (Frequency-Based) ===");
            System.Diagnostics.Debug.WriteLine($"Global max frequency: {maxFreq}");

            // Map each non-zero frequency to a color (light blue to dark blue/near-black)
            var colorMap = new Dictionary<int, Dictionary<int, Color>>();

            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                var dayColorMap = new Dictionary<int, Color>();

                if (context.FrequenciesPerDay.TryGetValue(dayIndex, out var dayFreqs))
                {
                    foreach (var kvp in dayFreqs)
                    {
                        int intervalIndex = kvp.Key;
                        int frequency = kvp.Value;

                        // Only map non-zero frequencies to colors
                        // Zero frequencies will be handled separately (white if within day's range)
                        if (frequency > 0)
                        {
                            // Normalize frequency to [0.0, 1.0] based on global maximum
                            double normalizedFreq = (double)frequency / maxFreq;

                            // Map to color (light blue = low frequency, dark blue/near-black = high frequency)
                            Color color = MapNormalizedValueToColor(normalizedFreq);
                            dayColorMap[intervalIndex] = color;
                        }
                        // Note: frequency = 0 is not added to dayColorMap here
                        // It will be handled separately based on whether interval overlaps day's range
                    }
                }

                colorMap[dayIndex] = dayColorMap;
            }

            return colorMap;
        }

        /// <summary>
        /// Calculates color for a specific interval using global frequency normalization.
        /// This ensures consistent shading across all intervals - a frequency of 1 will always be
        /// lighter than a frequency of 10, regardless of which interval they're in.
        /// </summary>
        public Color? CalculateIntervalColor(
            IntervalShadingContext context,
            int dayIndex,
            int intervalIndex,
            int intervalMaxFrequency,
            int globalMaxFrequency)
        {
            // Get frequency for this day/interval
            int frequency = 0;
            if (context.FrequenciesPerDay.TryGetValue(dayIndex, out var dayFreqs) &&
                dayFreqs.TryGetValue(intervalIndex, out var freq))
            {
                frequency = freq;
            }

            // Zero frequency intervals should be white (handled separately)
            if (frequency == 0)
                return null;

            // Normalize using GLOBAL max frequency to ensure consistent shading across all intervals
            // This matches the behavior of CalculateColorMap which also uses global normalization
            double normalizedFreq = globalMaxFrequency > 0
                ? (double)frequency / globalMaxFrequency
                : 0.0;

            return MapNormalizedValueToColor(normalizedFreq);
        }

        /// <summary>
        /// Maps a normalized value [0.0, 1.0] to a color gradient from light blue to dark blue/near-black.
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
            byte r = (byte)Math.Round(r0 + (r1 - r0) * normalizedValue);
            byte g = (byte)Math.Round(g0 + (g1 - g0) * normalizedValue);
            byte b = (byte)Math.Round(b0 + (b1 - b0) * normalizedValue);

            return Color.FromRgb(r, g, b);
        }
    }
}

