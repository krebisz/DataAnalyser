namespace DataVisualiser.Shared.Helpers;

/// <summary>
///     Helper class for calculating appropriate bin sizes and performing frequency binning.
/// </summary>
public static class FrequencyBinningHelper
{
    private const double TargetBinCount = 15.0;
    private const double MinimumBinCount = 5.0;
    private const double MaximumBinCount = 50.0;
    private const double BoundaryTolerance = 0.0001;

    /// <summary>
    ///     Calculates an appropriate bin size based on the value range.
    ///     Uses a "nice" bin size that's a power of 10, 2, or 5 times a power of 10.
    /// </summary>
    public static double CalculateBinSize(double minValue, double maxValue)
    {
        var range = maxValue - minValue;

        if (range <= 0 || double.IsNaN(range) || double.IsInfinity(range))
            return 1.0;

        var rawBinSize = range / TargetBinCount;
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(rawBinSize))));
        var normalized = rawBinSize / magnitude;
        var binSize = ChooseNiceMultiplier(normalized) * magnitude;

        var actualBinCount = (int)Math.Ceiling(range / binSize);
        if (actualBinCount < MinimumBinCount)
            return range / MinimumBinCount;
        if (actualBinCount > MaximumBinCount)
            return range / MaximumBinCount;

        return binSize;
    }

    /// <summary>
    ///     Creates bins for the given range and bin size.
    ///     Returns a list of (Min, Max) tuples for each bin.
    /// </summary>
    public static List<(double Min, double Max)> CreateBins(double minValue, double maxValue, double binSize)
    {
        var bins = new List<(double Min, double Max)>();
        var startBin = Math.Floor(minValue / binSize) * binSize;
        var current = startBin;

        while (current < maxValue)
        {
            bins.Add((current, current + binSize));
            current += binSize;
        }

        if (bins.Count > 0)
            bins[^1] = EnsureLastBinCoversMaximum(bins[^1], maxValue);

        return bins;
    }

    /// <summary>
    ///     Bins values and counts frequencies for each bin.
    ///     Returns a dictionary mapping bin index to frequency count.
    /// </summary>
    public static Dictionary<int, int> BinValuesAndCountFrequencies(List<double> values, List<(double Min, double Max)> bins)
    {
        var frequencies = InitializeFrequencyMap(bins.Count);

        foreach (var value in values)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                continue;

            var binIndex = FindBinIndex(value, bins);
            if (binIndex >= 0 && binIndex < bins.Count)
                frequencies[binIndex]++;
        }

        return frequencies;
    }

    /// <summary>
    ///     Normalizes frequency counts across all days and bins.
    ///     Returns normalized frequencies in range [0.0, 1.0] where 1.0 is the maximum frequency.
    /// </summary>
    public static Dictionary<int, Dictionary<int, double>> NormalizeFrequencies(Dictionary<int, Dictionary<int, int>> frequenciesPerDay)
    {
        var normalized = new Dictionary<int, Dictionary<int, double>>();
        var globalMaxFreq = Math.Max(1, frequenciesPerDay.Values.SelectMany(dayFreqs => dayFreqs.Values).DefaultIfEmpty(0).Max());

        foreach (var kvp in frequenciesPerDay)
        {
            var normalizedDayFreqs = new Dictionary<int, double>();

            foreach (var binFreq in kvp.Value)
                normalizedDayFreqs[binFreq.Key] = (double)binFreq.Value / globalMaxFreq;

            normalized[kvp.Key] = normalizedDayFreqs;
        }

        return normalized;
    }

    /// <summary>
    ///     Creates fixed-width intervals for the given range.
    ///     Returns a single interval when the range or interval count is invalid.
    /// </summary>
    public static List<(double Min, double Max)> CreateUniformIntervals(double minValue, double maxValue, int intervalCount)
    {
        var intervals = new List<(double Min, double Max)>();

        if (maxValue <= minValue || intervalCount <= 0)
        {
            intervals.Add((minValue, maxValue));
            return intervals;
        }

        var intervalSize = (maxValue - minValue) / intervalCount;

        for (var i = 0; i < intervalCount; i++)
        {
            var intervalMin = minValue + i * intervalSize;
            var intervalMax = i == intervalCount - 1 ? maxValue : intervalMin + intervalSize;
            intervals.Add((intervalMin, intervalMax));
        }

        return intervals;
    }

    /// <summary>
    ///     Counts values per interval for each bucket and ensures every bucket has an initialized frequency map.
    /// </summary>
    public static Dictionary<int, Dictionary<int, int>> CountFrequenciesPerBucket(Dictionary<int, List<double>> bucketValues, List<(double Min, double Max)> intervals, int bucketCount)
    {
        var result = new Dictionary<int, Dictionary<int, int>>(bucketCount);

        for (var bucketIndex = 0; bucketIndex < bucketCount; bucketIndex++)
        {
            var values = bucketValues.TryGetValue(bucketIndex, out var bucket) ? bucket : [];
            result[bucketIndex] = BinValuesAndCountFrequencies(values, intervals);
        }

        return result;
    }

    private static double ChooseNiceMultiplier(double normalized)
    {
        if (normalized <= 1.0)
            return 1.0;
        if (normalized <= 2.0)
            return 2.0;
        if (normalized <= 5.0)
            return 5.0;
        return 10.0;
    }

    private static (double Min, double Max) EnsureLastBinCoversMaximum((double Min, double Max) lastBin, double maxValue)
    {
        if (lastBin.Max < maxValue)
            return (lastBin.Min, maxValue);

        return lastBin;
    }

    private static Dictionary<int, int> InitializeFrequencyMap(int count)
    {
        var frequencies = new Dictionary<int, int>();

        for (var i = 0; i < count; i++)
            frequencies[i] = 0;

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

            if (IsWithinBin(value, bin, i == bins.Count - 1))
                return i;
        }

        if (value < bins[0].Min && value >= bins[0].Min - BoundaryTolerance)
            return 0;
        if (value > bins[^1].Max && value <= bins[^1].Max + BoundaryTolerance)
            return bins.Count - 1;

        return -1;
    }

    private static bool IsWithinBin(double value, (double Min, double Max) bin, bool isLastBin)
    {
        if (isLastBin)
            return value >= bin.Min && value <= bin.Max;

        return value >= bin.Min && value < bin.Max;
    }
}
