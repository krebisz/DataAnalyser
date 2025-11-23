using DataVisualiser.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVisualiser.Helper
{
    public static class MathHelper
    {
        public static TickInterval DetermineTickInterval(TimeSpan dateRange)
        {
            var totalDays = dateRange.TotalDays;
            var totalMonths = totalDays / 30.0;
            var totalYears = totalDays / 365.0;

            if (totalYears >= 2)
            {
                return TickInterval.Month;
            }
            else if (totalMonths >= 4)
            {
                return TickInterval.Week;
            }
            else if (totalMonths >= 1)
            {
                return TickInterval.Day;
            }
            else if (totalDays >= 14)
            {
                return TickInterval.Hour;
            }
            else
            {
                return TickInterval.Hour; // Default to hours for shorter ranges
            }
        }

        public static RecordToDayRatio DetermineRecordToDayRatio(decimal recordToDayRatioValue)
        {
            // Use explicit range checks to avoid unreachable switch cases
            if (recordToDayRatioValue <= 1m / 100000m)
                return RecordToDayRatio.Second;
            else if (recordToDayRatioValue <= 1m / 2000m)
                return RecordToDayRatio.Minute;
            else if (recordToDayRatioValue <= 1m / 50m)
                return RecordToDayRatio.Hour;
            else if (recordToDayRatioValue <= 1m / 2m)
                return RecordToDayRatio.Day;
            else if (recordToDayRatioValue <= 3m)
                return RecordToDayRatio.Week;
            else if (recordToDayRatioValue <= 8m)
                return RecordToDayRatio.Month;
            else
                return RecordToDayRatio.Year;
        }




        public static double CalculateSeparatorStep(TickInterval interval, int dataPointCount, TimeSpan dateRange)
        {
            // Calculate how many intervals we want to show
            double intervalsToShow = interval switch
            {
                TickInterval.Month => Math.Max(6, Math.Min(12, dateRange.TotalDays / 30.0)), // 6-12 months
                TickInterval.Week => Math.Max(4, Math.Min(8, dateRange.TotalDays / 7.0)),    // 4-8 weeks
                TickInterval.Day => Math.Max(7, Math.Min(14, dateRange.TotalDays)),          // 7-14 days
                TickInterval.Hour => Math.Max(12, Math.Min(24, dateRange.TotalHours)),        // 12-24 hours
                _ => 10
            };

            // Calculate step to achieve desired number of intervals
            var step = dataPointCount / intervalsToShow;

            // Ensure minimum step of 1
            return Math.Max(1.0, Math.Ceiling(step));
        }

        /// <summary>
        /// Generates normalized time intervals based on the tick interval type.
        /// Each interval represents a unique time unit (day/week/month/hour) from the start to end date.
        /// </summary>
        public static List<DateTime> GenerateNormalizedIntervals(DateTime fromDate, DateTime toDate, TickInterval interval)
        {
            var intervals = new List<DateTime>();

            if (fromDate > toDate)
                return intervals;

            DateTime current = NormalizeToIntervalStart(fromDate, interval);
            DateTime end = NormalizeToIntervalStart(toDate, interval);

            while (current <= end)
            {
                intervals.Add(current);
                current = IncrementInterval(current, interval);
            }

            // Always include the end date if it's not already included
            if (intervals.Count == 0 || intervals.Last() < toDate)
            {
                intervals.Add(end);
            }

            return intervals;
        }

        /// <summary>
        /// Normalizes a DateTime to the start of its interval based on the tick interval type.
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
        /// Increments a DateTime by one interval unit.
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
        /// Maps a timestamp to its corresponding normalized interval index.
        /// </summary>
        public static int MapTimestampToIntervalIndex(DateTime timestamp, List<DateTime> normalizedIntervals, TickInterval interval)
        {
            if (normalizedIntervals == null || normalizedIntervals.Count == 0)
                return 0;

            var normalizedTimestamp = NormalizeToIntervalStart(timestamp, interval);

            // Binary search for the interval
            int left = 0;
            int right = normalizedIntervals.Count - 1;
            int result = 0;

            while (left <= right)
            {
                int mid = (left + right) / 2;
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
        /// Fallback method: Creates smoothed data by grouping points by count (not time-based) to avoid overflow
        /// </summary>
        private static List<SmoothedDataPoint> CreateSmoothedDataByPointCount(List<HealthMetricData> data, int numberOfBins)
        {
            var smoothedPoints = new List<SmoothedDataPoint>();

            if (!data.Any() || numberOfBins <= 0)
            {
                return smoothedPoints;
            }

            var pointsPerBin = Math.Max(1, data.Count / numberOfBins);
            var bins = new List<List<HealthMetricData>>();

            for (int i = 0; i < data.Count; i += pointsPerBin)
            {
                var bin = data.Skip(i).Take(pointsPerBin).ToList();
                if (bin.Any())
                {
                    bins.Add(bin);
                }
            }

            // Calculate average for each bin
            foreach (var bin in bins)
            {
                var pointsInBin = bin.Where(p => p.Value.HasValue).ToList();
                if (pointsInBin.Any())
                {
                    // Calculate average value
                    var avgValue = pointsInBin.Select(p => (double)p.Value!.Value).Average();

                    // Use middle timestamp of the bin
                    var sortedBin = pointsInBin.OrderBy(p => p.NormalizedTimestamp).ToList();
                    var timestamp = sortedBin[sortedBin.Count / 2].NormalizedTimestamp;

                    smoothedPoints.Add(new SmoothedDataPoint
                    {
                        Timestamp = timestamp,
                        Value = avgValue
                    });
                }
            }

            return smoothedPoints.OrderBy(p => p.Timestamp).ToList();
        }

        /// <summary>
        /// Creates smoothed/averaged data by binning points into intervals with max 10 points per bin
        /// </summary>
        public static List<SmoothedDataPoint> CreateSmoothedData(List<HealthMetricData> data, DateTime fromDate, DateTime toDate)
        {
            var smoothedPoints = new List<SmoothedDataPoint>();

            if (!data.Any())
            {
                return smoothedPoints;
            }

            // Calculate bin size to ensure max 10 points per bin
            var totalPoints = data.Count;
            var numberOfBins = Math.Max(1, (int)Math.Ceiling(totalPoints / 10.0));
            var dateRange = toDate - fromDate;

            // Use double for tick calculations to avoid overflow
            // Handle very large date ranges by using a simpler approach
            double dateRangeTicks;
            double binSizeTicks;

            try
            {
                dateRangeTicks = dateRange.Ticks;
                binSizeTicks = dateRangeTicks / numberOfBins;

                // Safety check: if bin size calculation fails, use a simpler approach
                if (binSizeTicks <= 0 || double.IsInfinity(binSizeTicks) || double.IsNaN(binSizeTicks))
                {
                    // Fallback: use equal distribution of points instead of time-based bins
                    return CreateSmoothedDataByPointCount(data, numberOfBins);
                }
            }
            catch (OverflowException)
            {
                // If overflow occurs, use point-count based binning instead
                return CreateSmoothedDataByPointCount(data, numberOfBins);
            }

            var binSize = TimeSpan.FromTicks((long)binSizeTicks);

            // Group data points into bins
            var bins = new Dictionary<int, List<HealthMetricData>>();

            foreach (var point in data)
            {
                if (!point.Value.HasValue) continue;

                // Calculate which bin this point belongs to
                var timeOffset = point.NormalizedTimestamp - fromDate;
                var timeOffsetTicks = (double)timeOffset.Ticks;

                // Avoid division by zero
                if (binSizeTicks <= 0)
                {
                    continue;
                }

                var binIndex = (int)(timeOffsetTicks / binSizeTicks);

                // Ensure binIndex is within valid range
                binIndex = Math.Max(0, Math.Min(binIndex, numberOfBins - 1));

                if (!bins.ContainsKey(binIndex))
                {
                    bins[binIndex] = new List<HealthMetricData>();
                }
                bins[binIndex].Add(point);
            }

            // Calculate average for each bin
            foreach (var bin in bins.OrderBy(b => b.Key))
            {
                var pointsInBin = bin.Value;
                if (pointsInBin.Any())
                {
                    // Calculate average value
                    var avgValue = pointsInBin.Where(p => p.Value.HasValue).Select(p => (double)p.Value!.Value).Average();

                    // Calculate average timestamp using double to avoid overflow
                    var avgTimestampTicks = pointsInBin.Average(p => (double)p.NormalizedTimestamp.Ticks);

                    // Safety check for timestamp
                    if (double.IsInfinity(avgTimestampTicks) || double.IsNaN(avgTimestampTicks))
                    {
                        continue; // Skip this bin if timestamp calculation failed
                    }

                    var timestamp = new DateTime((long)avgTimestampTicks);

                    smoothedPoints.Add(new SmoothedDataPoint
                    {
                        Timestamp = timestamp,
                        Value = avgValue
                    });
                }
            }

            return smoothedPoints.OrderBy(p => p.Timestamp).ToList();
        }

        /// <summary>
        /// Interpolates smoothed data points to match raw data timestamp positions
        /// </summary>
        public static List<double> InterpolateSmoothedData(List<SmoothedDataPoint> smoothedData, List<DateTime> rawTimestamps)
        {
            var interpolatedValues = new List<double>();

            if (!smoothedData.Any() || !rawTimestamps.Any())
            {
                return interpolatedValues;
            }

            // Sort smoothed data by timestamp (only once)
            var sortedSmoothed = smoothedData.OrderBy(p => p.Timestamp).ToList();

            if (!sortedSmoothed.Any())
            {
                // No smoothed data - return all NaNs
                return rawTimestamps.Select(_ => double.NaN).ToList();
            }

            // Optimize: use binary search approach for better performance with large datasets
            foreach (var rawTimestamp in rawTimestamps)
            {
                // Find the two smoothed points that bracket this raw timestamp using binary search
                SmoothedDataPoint? lowerPoint = null;
                SmoothedDataPoint? upperPoint = null;

                // Binary search for the insertion point
                int left = 0;
                int right = sortedSmoothed.Count - 1;
                int insertIndex = sortedSmoothed.Count;

                while (left <= right)
                {
                    int mid = (left + right) / 2;
                    if (sortedSmoothed[mid].Timestamp <= rawTimestamp)
                    {
                        lowerPoint = sortedSmoothed[mid];
                        left = mid + 1;
                    }
                    else
                    {
                        upperPoint = sortedSmoothed[mid];
                        insertIndex = mid;
                        right = mid - 1;
                    }
                }

                // If we found a lower point but no upper point, the next point is the upper
                if (lowerPoint != null && upperPoint == null && insertIndex < sortedSmoothed.Count)
                {
                    upperPoint = sortedSmoothed[insertIndex];
                }

                // If we found an upper point but no lower point, the previous point is the lower
                if (upperPoint != null && lowerPoint == null && insertIndex > 0)
                {
                    lowerPoint = sortedSmoothed[insertIndex - 1];
                }

                double interpolatedValue;

                if (lowerPoint == null && upperPoint != null)
                {
                    // Before first smoothed point - use first value
                    interpolatedValue = upperPoint.Value;
                }
                else if (lowerPoint != null && upperPoint == null)
                {
                    // After last smoothed point - use last value
                    interpolatedValue = lowerPoint.Value;
                }
                else if (lowerPoint != null && upperPoint != null)
                {
                    if (lowerPoint.Timestamp == upperPoint.Timestamp)
                    {
                        // Exact match
                        interpolatedValue = lowerPoint.Value;
                    }
                    else
                    {
                        // Linear interpolation
                        var timeDiff = (rawTimestamp - lowerPoint.Timestamp).TotalMilliseconds;
                        var totalDiff = (upperPoint.Timestamp - lowerPoint.Timestamp).TotalMilliseconds;
                        var ratio = totalDiff > 0 ? timeDiff / totalDiff : 0;
                        interpolatedValue = lowerPoint.Value + (upperPoint.Value - lowerPoint.Value) * ratio;
                    }
                }
                else
                {
                    // No smoothed data available - use NaN
                    interpolatedValue = double.NaN;
                }

                interpolatedValues.Add(interpolatedValue);
            }

            return interpolatedValues;
        }

        /// <summary>
        /// Formats a number to 3 significant digits for display.
        /// Examples: 12345.678 → "12300", 0.0012345 → "0.00123", 123.456 → "123"
        /// </summary>
        public static string FormatToThreeSignificantDigits(double value)
        {
            // First round the value to 3 significant digits to prevent precision issues
            double rounded = RoundToThreeSignificantDigits(value);

            if (double.IsNaN(rounded) || double.IsInfinity(rounded))
            {
                return rounded.ToString();
            }

            if (rounded == 0)
            {
                return "0";
            }

            // Calculate the order of magnitude for formatting
            double absValue = Math.Abs(rounded);
            int orderOfMagnitude = (int)Math.Floor(Math.Log10(absValue));

            // Determine the number of decimal places needed
            // We want to show up to 3 significant digits, but avoid unnecessary trailing zeros
            int decimalPlaces = Math.Max(0, 2 - orderOfMagnitude);

            // Format the number
            if (decimalPlaces > 0)
            {
                // Remove trailing zeros
                string formatted = rounded.ToString($"F{decimalPlaces}", System.Globalization.CultureInfo.InvariantCulture);
                if (formatted.Contains('.'))
                {
                    formatted = formatted.TrimEnd('0').TrimEnd('.');
                }
                return formatted;
            }
            else
            {
                // For whole numbers, format without decimals
                return ((long)Math.Round(rounded)).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }


        /// <summary>
        /// Rounds a number to 3 significant digits and returns the rounded value.
        /// This is used for axis bounds to prevent floating point precision issues.
        /// </summary>
        public static double RoundToThreeSignificantDigits(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value == 0)
            {
                return value;
            }

            // Calculate the order of magnitude
            double absValue = Math.Abs(value);
            int orderOfMagnitude = (int)Math.Floor(Math.Log10(absValue));

            // Calculate the multiplier to get 3 significant digits
            double multiplier = Math.Pow(10, 2 - orderOfMagnitude);

            // Round to 3 significant digits
            return Math.Round(value * multiplier) / multiplier;
        }

        public static List<double>? ReturnValueDifferences(List<double>? valueList1, List<double>? valueList2)
        {
            var valueDiffernces = valueList1.Zip(valueList2, (a, b) => (double.IsNaN(a) || double.IsNaN(b)) ? double.NaN : (a - b)).ToList();
            return valueDiffernces;
        }

        public static List<double>? ReturnValueRatios(List<double>? valueList1, List<double>? valueLiat2)
        {
            var valueRatios = valueList1.Zip(valueLiat2, (a, b) => (double.IsNaN(a) || double.IsNaN(b) || b == 0.0) ? double.NaN : (a / b)).ToList();
            return valueRatios;
        }

    }
}
