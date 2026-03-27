using DataVisualiser.Shared.Models;

namespace DataVisualiser.Shared.Helpers;

internal static class TimeSeriesSmoothingHelper
{
    public static List<SmoothedDataPoint> CreateSmoothedData(List<MetricData> data, DateTime fromDate, DateTime toDate)
    {
        if (data == null || data.Count == 0)
            return new List<SmoothedDataPoint>();

        var numberOfBins = CalculateNumberOfBins(data.Count);

        if (!TryCalculateBinSize(fromDate, toDate, numberOfBins, out var binSizeTicks))
            return CreateSmoothedDataByPointCount(data, numberOfBins);

        var bins = BinDataByTime(data, fromDate, numberOfBins, binSizeTicks);
        var smoothedPoints = CreateAveragedPointsFromBins(bins);

        return smoothedPoints.OrderBy(point => point.Timestamp).ToList();
    }

    public static List<double> InterpolateSmoothedData(List<SmoothedDataPoint> smoothedData, List<DateTime> rawTimestamps)
    {
        if (smoothedData == null || smoothedData.Count == 0)
            return CreateNaNResults(rawTimestamps);

        if (rawTimestamps == null || rawTimestamps.Count == 0)
            return new List<double>();

        var sortedSmoothed = smoothedData.OrderBy(point => point.Timestamp).ToList();

        if (sortedSmoothed.Count == 0)
            return CreateNaNResults(rawTimestamps);

        var results = new List<double>(rawTimestamps.Count);

        foreach (var rawTimestamp in rawTimestamps)
            results.Add(InterpolateValueAtTimestamp(sortedSmoothed, rawTimestamp));

        return results;
    }

    private static List<SmoothedDataPoint> CreateSmoothedDataByPointCount(List<MetricData> data, int numberOfBins)
    {
        var smoothedPoints = new List<SmoothedDataPoint>();

        if (!data.Any() || numberOfBins <= 0)
            return smoothedPoints;

        var pointsPerBin = Math.Max(1, data.Count / numberOfBins);
        var bins = new List<List<MetricData>>();

        for (var i = 0; i < data.Count; i += pointsPerBin)
        {
            var bin = data.Skip(i).Take(pointsPerBin).ToList();
            if (bin.Any())
                bins.Add(bin);
        }

        foreach (var bin in bins)
        {
            var pointsInBin = bin.Where(point => point.Value.HasValue).ToList();
            if (!pointsInBin.Any())
                continue;

            var avgValue = pointsInBin.Select(point => (double)point.Value!.Value).Average();
            var sortedBin = pointsInBin.OrderBy(point => point.NormalizedTimestamp).ToList();
            var timestamp = sortedBin[sortedBin.Count / 2].NormalizedTimestamp;

            smoothedPoints.Add(new SmoothedDataPoint
            {
                    Timestamp = timestamp,
                    Value = avgValue
            });
        }

        return smoothedPoints.OrderBy(point => point.Timestamp).ToList();
    }

    private static double InterpolateValueAtTimestamp(List<SmoothedDataPoint> sortedSmoothed, DateTime targetTimestamp)
    {
        var (lower, upper) = FindBoundingPoints(sortedSmoothed, targetTimestamp);

        return CalculateInterpolatedValue(lower, upper, targetTimestamp);
    }

    private static (SmoothedDataPoint? Lower, SmoothedDataPoint? Upper) FindBoundingPoints(List<SmoothedDataPoint> sortedSmoothed, DateTime targetTimestamp)
    {
        SmoothedDataPoint? lower = null;
        SmoothedDataPoint? upper = null;

        var left = 0;
        var right = sortedSmoothed.Count - 1;
        var insertIndex = sortedSmoothed.Count;

        while (left <= right)
        {
            var mid = (left + right) / 2;

            if (sortedSmoothed[mid].Timestamp <= targetTimestamp)
            {
                lower = sortedSmoothed[mid];
                left = mid + 1;
            }
            else
            {
                upper = sortedSmoothed[mid];
                insertIndex = mid;
                right = mid - 1;
            }
        }

        if (lower != null && upper == null && insertIndex < sortedSmoothed.Count)
            upper = sortedSmoothed[insertIndex];

        if (upper != null && lower == null && insertIndex > 0)
            lower = sortedSmoothed[insertIndex - 1];

        return (lower, upper);
    }

    private static int CalculateNumberOfBins(int totalPoints)
    {
        return Math.Max(1, (int)Math.Ceiling(totalPoints / 10.0));
    }

    private static bool TryCalculateBinSize(DateTime fromDate, DateTime toDate, int numberOfBins, out double binSizeTicks)
    {
        binSizeTicks = 0;

        try
        {
            var dateRangeTicks = (double)(toDate - fromDate).Ticks;

            if (dateRangeTicks <= 0)
                return false;

            binSizeTicks = dateRangeTicks / numberOfBins;

            return binSizeTicks > 0 && !double.IsInfinity(binSizeTicks) && !double.IsNaN(binSizeTicks);
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    private static Dictionary<int, List<MetricData>> BinDataByTime(List<MetricData> data, DateTime fromDate, int numberOfBins, double binSizeTicks)
    {
        var bins = new Dictionary<int, List<MetricData>>();

        foreach (var point in data)
        {
            if (!point.Value.HasValue)
                continue;

            var binIndex = CalculateBinIndex(point.NormalizedTimestamp, fromDate, numberOfBins, binSizeTicks);

            if (!bins.TryGetValue(binIndex, out var bin))
            {
                bin = new List<MetricData>();
                bins[binIndex] = bin;
            }

            bin.Add(point);
        }

        return bins;
    }

    private static int CalculateBinIndex(DateTime timestamp, DateTime fromDate, int numberOfBins, double binSizeTicks)
    {
        if (binSizeTicks <= 0)
            return 0;

        var offsetTicks = (double)(timestamp - fromDate).Ticks;
        var rawIndex = (int)(offsetTicks / binSizeTicks);

        return Math.Clamp(rawIndex, 0, numberOfBins - 1);
    }

    private static List<SmoothedDataPoint> CreateAveragedPointsFromBins(Dictionary<int, List<MetricData>> bins)
    {
        var result = new List<SmoothedDataPoint>();

        foreach (var bin in bins.OrderBy(item => item.Key))
        {
            var averagedPoint = TryCreateAveragedPoint(bin.Value);
            if (averagedPoint != null)
                result.Add(averagedPoint);
        }

        return result;
    }

    private static SmoothedDataPoint? TryCreateAveragedPoint(List<MetricData> points)
    {
        if (points == null || points.Count == 0)
            return null;

        var validValues = points.Where(point => point.Value.HasValue).ToList();

        if (validValues.Count == 0)
            return null;

        var averageValue = validValues.Average(point => (double)point.Value!.Value);
        var averageTimestampTicks = validValues.Average(point => (double)point.NormalizedTimestamp.Ticks);

        if (double.IsNaN(averageTimestampTicks) || double.IsInfinity(averageTimestampTicks))
            return null;

        return new SmoothedDataPoint
        {
                Timestamp = new DateTime((long)averageTimestampTicks),
                Value = averageValue
        };
    }

    private static List<double> CreateNaNResults(List<DateTime> timestamps)
    {
        return timestamps == null ? new List<double>() : timestamps.Select(_ => double.NaN).ToList();
    }

    private static double CalculateInterpolatedValue(SmoothedDataPoint? lower, SmoothedDataPoint? upper, DateTime targetTimestamp)
    {
        if (lower == null && upper == null)
            return double.NaN;

        if (lower == null)
            return upper!.Value;

        if (upper == null)
            return lower.Value;

        if (lower.Timestamp == upper.Timestamp)
            return lower.Value;

        return LinearlyInterpolate(lower, upper, targetTimestamp);
    }

    private static double LinearlyInterpolate(SmoothedDataPoint lower, SmoothedDataPoint upper, DateTime targetTimestamp)
    {
        var elapsedMs = (targetTimestamp - lower.Timestamp).TotalMilliseconds;
        var totalMs = (upper.Timestamp - lower.Timestamp).TotalMilliseconds;

        if (totalMs <= 0)
            return lower.Value;

        var ratio = elapsedMs / totalMs;
        return lower.Value + (upper.Value - lower.Value) * ratio;
    }
}
