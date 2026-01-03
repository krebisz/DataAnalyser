using DataVisualiser.Shared.Models;
using System.Globalization;

namespace DataVisualiser.Shared.Helpers;

public static class MathHelper
{
    public static TickInterval DetermineTickInterval(TimeSpan dateRange)
    {
        var totalDays = dateRange.TotalDays;
        var totalMonths = totalDays / 30.0;
        var totalYears = totalDays / 365.0;

        if (totalYears >= 2)
            return TickInterval.Month;

        if (totalMonths >= 4)
            return TickInterval.Week;

        if (totalMonths >= 1)
            return TickInterval.Day;

        if (totalDays >= 14)
            return TickInterval.Hour;

        return TickInterval.Hour; // Default to hours for shorter ranges
    }

    /// <summary>
    ///     Calculates the optimal maximum number of records to fetch based on date range.
    ///     Prevents performance issues with extremely large datasets while maintaining visual quality.
    /// </summary>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns>Maximum records to fetch, or null for no limit (small datasets)</returns>
    public static int? CalculateOptimalMaxRecords(DateTime from, DateTime to)
    {
        var dateRange = to - from;
        var totalDays = dateRange.TotalDays;
        var totalHours = dateRange.TotalHours;

        // For very short ranges (< 1 day), no limit needed
        if (totalDays < 1)
            return null; // No limit

        // For daily data: ~1 record per day
        // 2 years = ~730 records (acceptable)
        // 10 years = ~3,650 records (should limit)
        if (totalDays <= 730) // ~2 years
            return null; // No limit for reasonable daily data

        // For hourly data: ~24 records per day
        // 2 years = ~17,520 records (should limit to ~10,000)
        // 1 year = ~8,760 records (acceptable)
        if (totalDays <= 365) // ~1 year
            return null; // No limit for 1 year of hourly data

        // For longer ranges, apply intelligent limits
        // Target: ~10,000 records max for good performance
        // Calculate sample rate to achieve this
        if (totalDays > 365)
        {
            // Estimate records per day based on resolution
            // Assume hourly data for long ranges (worst case)
            var estimatedRecordsPerDay = 24.0; // Hourly
            var estimatedTotalRecords = totalDays * estimatedRecordsPerDay;

            if (estimatedTotalRecords > 10000)
            {
                // Calculate sample rate to get ~10,000 records
                var sampleRate = (int)Math.Ceiling(estimatedTotalRecords / 10000.0);
                // Return null and let client-side handle it, or use SQL sampling
                // For now, use SQL TOP limit
                return 10000;
            }
        }

        return null; // No limit needed
    }

    public static RecordToDayRatio DetermineRecordToDayRatio(decimal recordToDayRatioValue)
    {
        // Use explicit range checks to avoid unreachable switch cases
        if (recordToDayRatioValue <= 1m / 100000m)
            return RecordToDayRatio.Second;
        if (recordToDayRatioValue <= 1m / 2000m)
            return RecordToDayRatio.Minute;
        if (recordToDayRatioValue <= 1m / 50m)
            return RecordToDayRatio.Hour;
        if (recordToDayRatioValue <= 1m / 2m)
            return RecordToDayRatio.Day;
        if (recordToDayRatioValue <= 3m)
            return RecordToDayRatio.Week;
        if (recordToDayRatioValue <= 8m)
            return RecordToDayRatio.Month;
        return RecordToDayRatio.Year;
    }


    public static double CalculateSeparatorStep(TickInterval interval, int dataPointCount, TimeSpan dateRange)
    {
        // Calculate how many intervals we want to show
        var intervalsToShow = interval switch
        {
            TickInterval.Month => Math.Max(6, Math.Min(12, dateRange.TotalDays / 30.0)), // 6-12 months
            TickInterval.Week => Math.Max(4, Math.Min(8, dateRange.TotalDays / 7.0)), // 4-8 weeks
            TickInterval.Day => Math.Max(7, Math.Min(14, dateRange.TotalDays)), // 7-14 days
            TickInterval.Hour => Math.Max(12, Math.Min(24, dateRange.TotalHours)), // 12-24 hours
            _ => 10
        };

        // Calculate step to achieve desired number of intervals
        var step = dataPointCount / intervalsToShow;

        // Ensure minimum step of 1
        return Math.Max(1.0, Math.Ceiling(step));
    }

    /// <summary>
    ///     Generates normalized time intervals based on the tick interval type.
    ///     Each interval represents a unique time unit (day/week/month/hour) from the start to end date.
    /// </summary>
    public static List<DateTime> GenerateNormalizedIntervals(DateTime fromDate, DateTime toDate, TickInterval interval)
    {
        var intervals = new List<DateTime>();

        if (fromDate > toDate)
            return intervals;

        var current = NormalizeToIntervalStart(fromDate, interval);
        var end = NormalizeToIntervalStart(toDate, interval);

        while (current <= end)
        {
            intervals.Add(current);
            current = IncrementInterval(current, interval);
        }

        // Always include the end date if it's not already included
        if (intervals.Count == 0 || intervals.Last() < toDate)
            intervals.Add(end);

        return intervals;
    }

    /// <summary>
    ///     Normalizes a DateTime to the start of its interval based on the tick interval type.
    /// </summary>
    private static DateTime NormalizeToIntervalStart(DateTime dateTime, TickInterval interval)
    {
        return interval switch
        {
            TickInterval.Month => new DateTime(dateTime.Year, dateTime.Month, 1),
            TickInterval.Week => dateTime.Date.AddDays(-(int)dateTime.DayOfWeek), // Start of week (Sunday)
            TickInterval.Day => dateTime.Date,
            TickInterval.Hour => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0),
            _ => dateTime.Date
        };
    }

    /// <summary>
    ///     Increments a DateTime by one interval unit.
    /// </summary>
    private static DateTime IncrementInterval(DateTime dateTime, TickInterval interval)
    {
        return interval switch
        {
            TickInterval.Month => dateTime.AddMonths(1),
            TickInterval.Week => dateTime.AddDays(7),
            TickInterval.Day => dateTime.AddDays(1),
            TickInterval.Hour => dateTime.AddHours(1),
            _ => dateTime.AddDays(1)
        };
    }

    /// <summary>
    ///     Maps a timestamp to its corresponding normalized interval index.
    /// </summary>
    public static int MapTimestampToIntervalIndex(DateTime timestamp, List<DateTime> normalizedIntervals, TickInterval interval)
    {
        if (normalizedIntervals == null || normalizedIntervals.Count == 0)
            return 0;

        var normalizedTimestamp = NormalizeToIntervalStart(timestamp, interval);

        // Binary search for the interval
        var left = 0;
        var right = normalizedIntervals.Count - 1;
        var result = 0;

        while (left <= right)
        {
            var mid = (left + right) / 2;
            if (normalizedIntervals[mid] <= normalizedTimestamp)
            {
                result = mid;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        // Ensure result is within bounds
        return Math.Max(0, Math.Min(result, normalizedIntervals.Count - 1));
    }

    /// <summary>
    ///     Fallback method: Creates smoothed data by grouping points by count (not time-based) to avoid overflow
    /// </summary>
    private static List<SmoothedDataPoint> CreateSmoothedDataByPointCount(List<HealthMetricData> data, int numberOfBins)
    {
        var smoothedPoints = new List<SmoothedDataPoint>();

        if (!data.Any() || numberOfBins <= 0)
            return smoothedPoints;

        var pointsPerBin = Math.Max(1, data.Count / numberOfBins);
        var bins = new List<List<HealthMetricData>>();

        for (var i = 0; i < data.Count; i += pointsPerBin)
        {
            var bin = data.Skip(i).
                Take(pointsPerBin).
                ToList();
            if (bin.Any())
                bins.Add(bin);
        }

        // Calculate average for each bin
        foreach (var bin in bins)
        {
            var pointsInBin = bin.Where(p => p.Value.HasValue).
                ToList();
            if (pointsInBin.Any())
            {
                // Calculate average value
                var avgValue = pointsInBin.Select(p => (double)p.Value!.Value).
                    Average();

                // Use middle timestamp of the bin
                var sortedBin = pointsInBin.OrderBy(p => p.NormalizedTimestamp).
                    ToList();
                var timestamp = sortedBin[sortedBin.Count / 2].NormalizedTimestamp;

                smoothedPoints.Add(new SmoothedDataPoint
                {
                    Timestamp = timestamp,
                    Value = avgValue
                });
            }
        }

        return smoothedPoints.OrderBy(p => p.Timestamp).
            ToList();
    }

    /// <summary>
    ///     Creates smoothed/averaged data by binning points into intervals
    ///     with a maximum of 10 points per bin.
    /// </summary>
    public static List<SmoothedDataPoint> CreateSmoothedData(List<HealthMetricData> data, DateTime fromDate, DateTime toDate)
    {
        if (data == null || data.Count == 0)
            return new List<SmoothedDataPoint>();

        var numberOfBins = CalculateNumberOfBins(data.Count);

        if (!TryCalculateBinSize(fromDate, toDate, numberOfBins, out var binSizeTicks))
            return CreateSmoothedDataByPointCount(data, numberOfBins);

        var bins = BinDataByTime(data, fromDate, numberOfBins, binSizeTicks);
        var smoothedPoints = CreateAveragedPointsFromBins(bins);

        return smoothedPoints.OrderBy(p => p.Timestamp).
            ToList();
    }

    /// <summary>
    ///     Interpolates smoothed data points to match raw data timestamp positions
    /// </summary>
    public static List<double> InterpolateSmoothedData(List<SmoothedDataPoint> smoothedData, List<DateTime> rawTimestamps)
    {
        if (smoothedData == null || smoothedData.Count == 0)
            return CreateNaNResults(rawTimestamps);

        if (rawTimestamps == null || rawTimestamps.Count == 0)
            return new List<double>();

        var sortedSmoothed = SortSmoothedData(smoothedData);

        if (sortedSmoothed.Count == 0)
            return CreateNaNResults(rawTimestamps);

        var results = new List<double>(rawTimestamps.Count);

        foreach (var rawTimestamp in rawTimestamps)
        {
            var value = InterpolateValueAtTimestamp(sortedSmoothed, rawTimestamp);
            results.Add(value);
        }

        return results;
    }

    #region Interpolation Core

    private static double InterpolateValueAtTimestamp(List<SmoothedDataPoint> sortedSmoothed, DateTime targetTimestamp)
    {
        var (lower, upper) = FindBoundingPoints(sortedSmoothed, targetTimestamp);

        return CalculateInterpolatedValue(lower, upper, targetTimestamp);
    }

    #endregion

    #region Binary Search

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

        // Resolve missing neighbor using insertion index
        if (lower != null && upper == null && insertIndex < sortedSmoothed.Count)
            upper = sortedSmoothed[insertIndex];

        if (upper != null && lower == null && insertIndex > 0)
            lower = sortedSmoothed[insertIndex - 1];

        return (lower, upper);
    }

    #endregion


    /// <summary>
    ///     Formats a number to 3 significant digits for display.
    ///     Examples: 12345.678 → "12300", 0.0012345 → "0.00123", 123.456 → "123"
    /// </summary>
    public static string FormatToThreeSignificantDigits(double value)
    {
        // First round the value to 3 significant digits to prevent precision issues
        var rounded = RoundToThreeSignificantDigits(value);

        if (double.IsNaN(rounded) || double.IsInfinity(rounded))
            return rounded.ToString();

        if (rounded == 0)
            return "0";

        // Calculate the order of magnitude for formatting
        var absValue = Math.Abs(rounded);
        var orderOfMagnitude = (int)Math.Floor(Math.Log10(absValue));

        // Determine the number of decimal places needed
        // We want to show up to 3 significant digits, but avoid unnecessary trailing zeros
        var decimalPlaces = Math.Max(0, 2 - orderOfMagnitude);

        // Format the number
        if (decimalPlaces > 0)
        {
            // Remove trailing zeros
            var formatted = rounded.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture);
            if (formatted.Contains('.'))
                formatted = formatted.TrimEnd('0').
                    TrimEnd('.');
            return formatted;
        }

        // For whole numbers, format without decimals
        return ((long)Math.Round(rounded)).ToString(CultureInfo.InvariantCulture);
    }


    /// <summary>
    ///     Rounds a number to 3 significant digits and returns the rounded value.
    ///     This is used for axis bounds to prevent floating point precision issues.
    /// </summary>
    public static double RoundToThreeSignificantDigits(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value == 0)
            return value;

        // Calculate the order of magnitude
        var absValue = Math.Abs(value);
        var orderOfMagnitude = (int)Math.Floor(Math.Log10(absValue));

        // Calculate the multiplier to get 3 significant digits
        var multiplier = Math.Pow(10, 2 - orderOfMagnitude);

        // Round to 3 significant digits
        return Math.Round(value * multiplier) / multiplier;
    }

    public static List<double>? ReturnValueNormalized(List<double>? values)
    {
        if (values == null || values.Count == 0)
            return null;

        var min = values.Where(v => !double.IsNaN(v)).
            DefaultIfEmpty(double.NaN).
            Min();
        var max = values.Where(v => !double.IsNaN(v)).
            DefaultIfEmpty(double.NaN).
            Max();

        // Avoid zero-range situations (all values identical)
        if (double.IsNaN(min) || double.IsNaN(max) || min == max)
            return values.Select(v => double.NaN).
                ToList();

        return values.Select(v => double.IsNaN(v) ? double.NaN : (v - min) / (max - min)).
            ToList();
    }

    public static List<double>? ReturnValueNormalized(List<double>? values, NormalizationMode mode = NormalizationMode.ZeroToOne)
    {
        if (mode == NormalizationMode.RelativeToMax)
            throw new InvalidOperationException("RelativeToMax requires two lists. Use the overload.");

        if (values == null || values.Count == 0)
            return null;

        var valid = values.Where(v => !double.IsNaN(v)).
            ToList();

        if (valid.Count == 0)
            return values.Select(_ => double.NaN).
                ToList();

        var min = valid.Min();
        var max = valid.Max();

        if (double.IsNaN(min) || double.IsNaN(max) || min == max)
            return values.Select(_ => double.NaN).
                ToList();

        return mode switch
        {
            NormalizationMode.ZeroToOne => values.Select(v => double.IsNaN(v) ? double.NaN : (v - min) / (max - min)).
                ToList(),

            NormalizationMode.PercentageOfMax => values.Select(v => double.IsNaN(v) ? double.NaN : v / max * 100.0).
                ToList(),

            _ => throw new NotSupportedException()
        };
    }


    public static (List<double>? FirstNormalized, List<double>? SecondNormalized) ReturnValueNormalized(List<double>? first, List<double>? second, NormalizationMode mode)
    {
        if (mode != NormalizationMode.RelativeToMax)
            throw new NotSupportedException("This overload only supports RelativeToMax.");

        if (first == null || second == null)
            return (null, null);

        var count = Math.Min(first.Count, second.Count);

        // First normalize each list independently using PercentageOfMax
        var firstPercent = ReturnValueNormalized(first, NormalizationMode.PercentageOfMax);
        var secondPercent = ReturnValueNormalized(second, NormalizationMode.PercentageOfMax);

        if (firstPercent == null || secondPercent == null)
            return (null, null);

        var relative = new List<double>(count);

        for (var i = 0; i < count; i++)
        {
            var a = firstPercent[i];
            var b = secondPercent[i];

            if (double.IsNaN(a) || double.IsNaN(b) || b == 0)
            {
                relative.Add(double.NaN);
                continue;
            }

            // a and b are both in percentage units already
            // e.g., a=80, b=80 => (80/80)*100 = 100
            relative.Add(a / b * 100.0);
        }

        // Second list is always a straight 100% line
        var straight100 = Enumerable.Repeat(100.0, count).
            ToList();

        return (relative, straight100);
    }


    /// <summary>
    ///     Calculates differences between two value lists (valueList1 - valueList2).
    /// </summary>
    public static List<double>? ReturnValueDifferences(List<double>? valueList1, List<double>? valueList2)
    {
        return ApplyBinaryOperation(valueList1, valueList2, (a, b) => a - b);
    }

    /// <summary>
    ///     Calculates ratios between two value lists (valueList1 / valueList2).
    /// </summary>
    public static List<double>? ReturnValueRatios(List<double>? valueList1, List<double>? valueList2)
    {
        if (valueList1 == null || valueList2 == null)
            return null;

        return ApplyBinaryOperation(valueList1, valueList2, (a, b) =>
        {
            if (b == 0.0)
                return double.NaN;
            return a / b;
        });
    }

    public static List<double> ApplyBinaryOperation(List<double>? list1, List<double>? list2, Func<double, double, double> operation)
    {
        if (list1 == null || list2 == null)
            return new List<double>();

        var count = Math.Min(list1.Count, list2.Count);

        var result = new List<double>(count);

        for (var i = 0; i < count; i++)
        {
            var a = list1[i];
            var b = list2[i];

            // Maintain NaN and Infinity semantics
            if (double.IsNaN(a) || double.IsNaN(b) || double.IsInfinity(a) || double.IsInfinity(b))
            {
                result.Add(double.NaN);
                continue;
            }

            try
            {
                var value = operation(a, b);
                // Check if operation resulted in invalid value
                if (double.IsNaN(value) || double.IsInfinity(value))
                    result.Add(double.NaN);
                else
                    result.Add(value);
            }
            catch
            {
                result.Add(double.NaN);
            }
        }

        return result;
    }

    /// <summary>
    ///     Applies a unary operation to a list of values.
    /// </summary>
    public static List<double> ApplyUnaryOperation(List<double>? list, Func<double, double> operation)
    {
        if (list == null)
            return new List<double>();

        var result = new List<double>(list.Count);

        foreach (var value in list)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                result.Add(double.NaN);
                continue;
            }

            try
            {
                var computed = operation(value);
                if (double.IsNaN(computed) || double.IsInfinity(computed))
                    result.Add(double.NaN);
                else
                    result.Add(computed);
            }
            catch
            {
                result.Add(double.NaN);
            }
        }

        return result;
    }

    #region Bin Calculation

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

    #endregion

    #region Binning Logic

    private static Dictionary<int, List<HealthMetricData>> BinDataByTime(List<HealthMetricData> data, DateTime fromDate, int numberOfBins, double binSizeTicks)
    {
        var bins = new Dictionary<int, List<HealthMetricData>>();

        foreach (var point in data)
        {
            if (!point.Value.HasValue)
                continue;

            var binIndex = CalculateBinIndex(point.NormalizedTimestamp, fromDate, numberOfBins, binSizeTicks);

            if (!bins.TryGetValue(binIndex, out var bin))
            {
                bin = new List<HealthMetricData>();
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

    #endregion

    #region Aggregation

    private static List<SmoothedDataPoint> CreateAveragedPointsFromBins(Dictionary<int, List<HealthMetricData>> bins)
    {
        var result = new List<SmoothedDataPoint>();

        foreach (var bin in bins.OrderBy(b => b.Key))
        {
            var averagedPoint = TryCreateAveragedPoint(bin.Value);
            if (averagedPoint != null)
                result.Add(averagedPoint);
        }

        return result;
    }

    private static SmoothedDataPoint? TryCreateAveragedPoint(List<HealthMetricData> points)
    {
        if (points == null || points.Count == 0)
            return null;

        var validValues = points.Where(p => p.Value.HasValue).
            ToList();

        if (validValues.Count == 0)
            return null;

        var averageValue = validValues.Average(p => (double)p.Value!.Value);

        var averageTimestampTicks = validValues.Average(p => (double)p.NormalizedTimestamp.Ticks);

        if (double.IsNaN(averageTimestampTicks) || double.IsInfinity(averageTimestampTicks))
            return null;

        return new SmoothedDataPoint
        {
            Timestamp = new DateTime((long)averageTimestampTicks),
            Value = averageValue
        };
    }

    #endregion

    #region Preparation

    private static List<SmoothedDataPoint> SortSmoothedData(List<SmoothedDataPoint> smoothedData)
    {
        return smoothedData.OrderBy(p => p.Timestamp).
            ToList();
    }

    private static List<double> CreateNaNResults(List<DateTime> timestamps)
    {
        return timestamps == null ? new List<double>() : timestamps.Select(_ => double.NaN).
            ToList();
    }

    #endregion

    #region Value Calculation

    private static double CalculateInterpolatedValue(SmoothedDataPoint? lower, SmoothedDataPoint? upper, DateTime targetTimestamp)
    {
        if (lower == null && upper == null)
            return double.NaN;

        if (lower == null)
            // Before first smoothed point
            return upper!.Value;

        if (upper == null)
            // After last smoothed point
            return lower.Value;

        if (lower.Timestamp == upper.Timestamp)
            // Exact match or degenerate case
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

    #endregion
}
