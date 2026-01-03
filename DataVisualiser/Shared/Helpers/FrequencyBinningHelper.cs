namespace DataVisualiser.Shared.Helpers;

/// <summary>
///     Helper class for calculating appropriate bin sizes and performing frequency binning.
/// </summary>
public static class FrequencyBinningHelper
{
    /// <summary>
    ///     Calculates an appropriate bin size based on the value range.
    ///     Uses a "nice" bin size that's a power of 10, 2, or 5 times a power of 10.
    ///     Example: range 0.7 → bin size 0.1
    /// </summary>
    public static double CalculateBinSize(double minValue, double maxValue)
    {
        var range = maxValue - minValue;

        if (range <= 0 || double.IsNaN(range) || double.IsInfinity(range))
            return 1.0;

        // Target: roughly 10-20 bins for good granularity
        var targetBinCount = 15.0;
        var rawBinSize = range / targetBinCount;

        // Find the order of magnitude
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(rawBinSize))));

        // Normalize to 1-10 range
        var normalized = rawBinSize / magnitude;

        // Choose a "nice" number: 1, 2, 5, or 10
        double niceMultiplier;
        if (normalized <= 1.0)
            niceMultiplier = 1.0;
        else if (normalized <= 2.0)
            niceMultiplier = 2.0;
        else if (normalized <= 5.0)
            niceMultiplier = 5.0;
        else
            niceMultiplier = 10.0;

        var binSize = niceMultiplier * magnitude;

        // Ensure we have at least 5 bins and at most 50 bins
        var actualBinCount = (int)Math.Ceiling(range / binSize);
        if (actualBinCount < 5)
            binSize = range / 5.0;
        else if (actualBinCount > 50)
            binSize = range / 50.0;

        return binSize;
    }

    /// <summary>
    ///     Creates bins for the given range and bin size.
    ///     Returns a list of (Min, Max) tuples for each bin.
    /// </summary>
    public static List<(double Min, double Max)> CreateBins(double minValue, double maxValue, double binSize)
    {
        var bins = new List<(double Min, double Max)>();

        // Start from the floor of minValue rounded down to nearest bin boundary
        var startBin = Math.Floor(minValue / binSize) * binSize;

        var current = startBin;
        while (current < maxValue)
        {
            var binMin = current;
            var binMax = current + binSize;
            bins.Add((binMin, binMax));
            current += binSize;
        }

        // Ensure the last bin includes maxValue
        if (bins.Count > 0)
        {
            var lastBin = bins[bins.Count - 1];
            if (lastBin.Max < maxValue)
                bins[bins.Count - 1] = (lastBin.Min, maxValue);
        }

        return bins;
    }

    /// <summary>
    ///     Bins values and counts frequencies for each bin.
    ///     Returns a dictionary mapping bin index to frequency count.
    /// </summary>
    public static Dictionary<int, int> BinValuesAndCountFrequencies(List<double> values, List<(double Min, double Max)> bins)
    {
        var frequencies = new Dictionary<int, int>();

        // Initialize all bins to 0
        for (var i = 0; i < bins.Count; i++)
            frequencies[i] = 0;

        foreach (var value in values)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                continue;

            // Find which bin this value belongs to
            var binIndex = FindBinIndex(value, bins);
            if (binIndex >= 0 && binIndex < bins.Count)
                frequencies[binIndex]++;
        }

        return frequencies;
    }

    /// <summary>
    ///     Finds the bin index for a given value.
    ///     Returns -1 if value is outside all bins.
    /// </summary>
    private static int FindBinIndex(double value, List<(double Min, double Max)> bins)
    {
        if (bins == null || bins.Count == 0)
            return -1;

        for (var i = 0; i < bins.Count; i++)
        {
            var bin = bins[i];
            // Check if value is in [Min, Max) for all bins except the last, which is [Min, Max]
            if (i < bins.Count - 1)
            {
                if (value >= bin.Min && value < bin.Max)
                    return i;
            }
            else
            {
                // Last bin is inclusive on both ends to catch maxValue
                if (value >= bin.Min && value <= bin.Max)
                    return i;
            }
        }

        // If value is slightly outside due to floating point precision, try to find nearest bin
        // This handles edge cases where value might be just outside due to rounding
        if (value < bins[0].Min && value >= bins[0].Min - 0.0001)
            return 0;
        if (value > bins[bins.Count - 1].Max && value <= bins[bins.Count - 1].Max + 0.0001)
            return bins.Count - 1;

        return -1;
    }

    /// <summary>
    ///     Normalizes frequency counts across all days and bins.
    ///     Returns normalized frequencies in range [0.0, 1.0] where 1.0 is the maximum frequency.
    /// </summary>
    public static Dictionary<int, Dictionary<int, double>> NormalizeFrequencies(Dictionary<int, Dictionary<int, int>> frequenciesPerDay)
    {
        var normalized = new Dictionary<int, Dictionary<int, double>>();

        // Find global maximum frequency across all days and bins
        var globalMaxFreq = 0;
        foreach (var dayFreqs in frequenciesPerDay.Values)
            foreach (var freq in dayFreqs.Values)
                if (freq > globalMaxFreq)
                    globalMaxFreq = freq;

        if (globalMaxFreq == 0)
            globalMaxFreq = 1; // Avoid division by zero

        // Normalize each day's frequencies
        foreach (var kvp in frequenciesPerDay)
        {
            var dayIndex = kvp.Key;
            var dayFreqs = kvp.Value;
            var normalizedDayFreqs = new Dictionary<int, double>();

            foreach (var binFreq in dayFreqs)
            {
                var binIndex = binFreq.Key;
                var frequency = binFreq.Value;
                var normalizedFreq = (double)frequency / globalMaxFreq;
                normalizedDayFreqs[binIndex] = normalizedFreq;
            }

            normalized[dayIndex] = normalizedDayFreqs;
        }

        return normalized;
    }
}