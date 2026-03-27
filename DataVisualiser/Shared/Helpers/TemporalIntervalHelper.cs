using DataVisualiser.Shared.Models;

namespace DataVisualiser.Shared.Helpers;

internal static class TemporalIntervalHelper
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

        return TickInterval.Hour;
    }

    public static double CalculateSeparatorStep(TickInterval interval, int dataPointCount, TimeSpan dateRange)
    {
        var intervalsToShow = interval switch
        {
                TickInterval.Month => Math.Max(6, Math.Min(12, dateRange.TotalDays / 30.0)),
                TickInterval.Week => Math.Max(4, Math.Min(8, dateRange.TotalDays / 7.0)),
                TickInterval.Day => Math.Max(7, Math.Min(14, dateRange.TotalDays)),
                TickInterval.Hour => Math.Max(12, Math.Min(24, dateRange.TotalHours)),
                _ => 10
        };

        var step = dataPointCount / intervalsToShow;

        return Math.Max(1.0, Math.Ceiling(step));
    }

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

        if (intervals.Count == 0 || intervals[^1] < toDate)
            intervals.Add(end);

        return intervals;
    }

    public static int MapTimestampToIntervalIndex(DateTime timestamp, IReadOnlyList<DateTime>? normalizedIntervals, TickInterval interval)
    {
        if (normalizedIntervals == null || normalizedIntervals.Count == 0)
            return 0;

        var normalizedTimestamp = NormalizeToIntervalStart(timestamp, interval);
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

        return Math.Max(0, Math.Min(result, normalizedIntervals.Count - 1));
    }

    private static DateTime NormalizeToIntervalStart(DateTime dateTime, TickInterval interval)
    {
        return interval switch
        {
                TickInterval.Month => new DateTime(dateTime.Year, dateTime.Month, 1),
                TickInterval.Week => dateTime.Date.AddDays(-(int)dateTime.DayOfWeek),
                TickInterval.Day => dateTime.Date,
                TickInterval.Hour => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0),
                _ => dateTime.Date
        };
    }

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
}
