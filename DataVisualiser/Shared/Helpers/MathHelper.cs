using System.Globalization;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Shared.Helpers;

public static class MathHelper
{
    public static TickInterval DetermineTickInterval(TimeSpan dateRange)
    {
        return TemporalIntervalHelper.DetermineTickInterval(dateRange);
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

        if (totalDays < ComputationDefaults.SqlLimitingMinDaysNoLimit)
            return null;

        if (totalDays <= ComputationDefaults.SqlLimitingMaxDailyDaysNoLimit)
            return null;

        if (totalDays <= ComputationDefaults.SqlLimitingMaxHourlyDaysNoLimit)
            return null;

        if (totalDays > 365)
        {
            var estimatedRecordsPerDay = ComputationDefaults.SqlLimitingEstimatedRecordsPerDay;
            var estimatedTotalRecords = totalDays * estimatedRecordsPerDay;

            if (estimatedTotalRecords > ComputationDefaults.SqlLimitingMaxRecords)
                return ComputationDefaults.SqlLimitingMaxRecords;
        }

        return null;
    }

    public static RecordToDayRatio DetermineRecordToDayRatio(decimal recordToDayRatioValue)
    {
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
        return TemporalIntervalHelper.CalculateSeparatorStep(interval, dataPointCount, dateRange);
    }

    /// <summary>
    ///     Generates normalized time intervals based on the tick interval type.
    ///     Each interval represents a unique time unit (day/week/month/hour) from the start to end date.
    /// </summary>
    public static List<DateTime> GenerateNormalizedIntervals(DateTime fromDate, DateTime toDate, TickInterval interval)
    {
        return TemporalIntervalHelper.GenerateNormalizedIntervals(fromDate, toDate, interval);
    }

    /// <summary>
    ///     Maps a timestamp to its corresponding normalized interval index.
    /// </summary>
    public static int MapTimestampToIntervalIndex(DateTime timestamp, List<DateTime> normalizedIntervals, TickInterval interval)
    {
        return TemporalIntervalHelper.MapTimestampToIntervalIndex(timestamp, normalizedIntervals, interval);
    }

    /// <summary>
    ///     Creates smoothed/averaged data by binning points into intervals
    ///     with a maximum of 10 points per bin.
    /// </summary>
    public static List<SmoothedDataPoint> CreateSmoothedData(List<MetricData> data, DateTime fromDate, DateTime toDate)
    {
        return TimeSeriesSmoothingHelper.CreateSmoothedData(data, fromDate, toDate);
    }

    /// <summary>
    ///     Interpolates smoothed data points to match raw data timestamp positions
    /// </summary>
    public static List<double> InterpolateSmoothedData(List<SmoothedDataPoint> smoothedData, List<DateTime> rawTimestamps)
    {
        return TimeSeriesSmoothingHelper.InterpolateSmoothedData(smoothedData, rawTimestamps);
    }

    /// <summary>
    ///     Formats a number to 3 significant digits for display.
    /// </summary>
    public static string FormatToThreeSignificantDigits(double value)
    {
        var rounded = RoundToThreeSignificantDigits(value);

        if (double.IsNaN(rounded) || double.IsInfinity(rounded))
            return rounded.ToString();

        if (rounded == 0)
            return "0";

        var absValue = Math.Abs(rounded);
        var orderOfMagnitude = (int)Math.Floor(Math.Log10(absValue));
        var decimalPlaces = Math.Max(0, 2 - orderOfMagnitude);

        if (decimalPlaces > 0)
        {
            var formatted = rounded.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture);
            if (formatted.Contains('.'))
                formatted = formatted.TrimEnd('0').TrimEnd('.');
            return formatted;
        }

        return ((long)Math.Round(rounded)).ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Formats decimal values to 3 significant non-zero digits after the decimal point (truncated).
    /// </summary>
    public static string FormatDisplayedValue(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return value.ToString(CultureInfo.InvariantCulture);

        if (value == 0)
            return "0";

        var sign = value < 0 ? "-" : string.Empty;
        var abs = Math.Abs(value);

        if (abs > (double)decimal.MaxValue)
            return value.ToString(CultureInfo.InvariantCulture);

        var absDecimal = (decimal)abs;
        var integerPart = decimal.Truncate(absDecimal);
        var fraction = absDecimal - integerPart;
        var integerText = integerPart.ToString(CultureInfo.InvariantCulture);

        if (fraction == 0m)
            return $"{sign}{integerText}";

        var digits = new List<char>();
        var nonZeroCount = 0;
        const int maxDigits = 18;

        while (fraction > 0m && nonZeroCount < 3 && digits.Count < maxDigits)
        {
            fraction *= 10m;
            var digit = (int)decimal.Truncate(fraction);
            fraction -= digit;
            digits.Add((char)('0' + digit));
            if (digit != 0)
                nonZeroCount++;
        }

        while (digits.Count > 0 && digits[^1] == '0')
            digits.RemoveAt(digits.Count - 1);

        if (digits.Count == 0)
            return $"{sign}{integerText}";

        return $"{sign}{integerText}.{new string(digits.ToArray())}";
    }

    /// <summary>
    ///     Rounds a number to 3 significant digits and returns the rounded value.
    /// </summary>
    public static double RoundToThreeSignificantDigits(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value == 0)
            return value;

        var absValue = Math.Abs(value);
        var orderOfMagnitude = (int)Math.Floor(Math.Log10(absValue));
        var multiplier = Math.Pow(10, 2 - orderOfMagnitude);

        return Math.Round(value * multiplier) / multiplier;
    }

    public static List<double>? ReturnValueNormalized(List<double>? values)
    {
        if (values == null || values.Count == 0)
            return null;

        var min = values.Where(v => !double.IsNaN(v)).DefaultIfEmpty(double.NaN).Min();
        var max = values.Where(v => !double.IsNaN(v)).DefaultIfEmpty(double.NaN).Max();

        if (double.IsNaN(min) || double.IsNaN(max) || min == max)
            return values.Select(v => double.NaN).ToList();

        return values.Select(v => double.IsNaN(v) ? double.NaN : (v - min) / (max - min)).ToList();
    }

    public static List<double>? ReturnValueNormalized(List<double>? values, NormalizationMode mode = NormalizationMode.ZeroToOne)
    {
        if (mode == NormalizationMode.RelativeToMax)
            throw new InvalidOperationException("RelativeToMax requires two lists. Use the overload.");

        if (values == null || values.Count == 0)
            return null;

        var valid = values.Where(v => !double.IsNaN(v)).ToList();

        if (valid.Count == 0)
            return values.Select(_ => double.NaN).ToList();

        var min = valid.Min();
        var max = valid.Max();

        if (double.IsNaN(min) || double.IsNaN(max) || min == max)
            return values.Select(_ => double.NaN).ToList();

        return mode switch
        {
                NormalizationMode.ZeroToOne => values.Select(v => double.IsNaN(v) ? double.NaN : (v - min) / (max - min)).ToList(),
                NormalizationMode.PercentageOfMax => values.Select(v => double.IsNaN(v) ? double.NaN : v / max * 100.0).ToList(),
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

            relative.Add(a / b * 100.0);
        }

        var straight100 = Enumerable.Repeat(100.0, count).ToList();

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

        return ApplyBinaryOperation(valueList1,
                valueList2,
                (a, b) =>
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

            if (double.IsNaN(a) || double.IsNaN(b) || double.IsInfinity(a) || double.IsInfinity(b))
            {
                result.Add(double.NaN);
                continue;
            }

            try
            {
                var value = operation(a, b);
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
}
