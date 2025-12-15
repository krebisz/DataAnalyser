using System;
using System.Collections.Generic;
using System.Linq;
using DataVisualiser.Models;

namespace DataVisualiser.Charts.Strategies
{
    public sealed class WeekdayTrendStrategy
    {
        public WeekdayTrendResult Compute(
            IEnumerable<HealthMetricData> data,
            DateTime from,
            DateTime to)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Filter to date range + ensure Value exists
            var filtered = data
                .Where(d => d != null && d.Value.HasValue)
                .Where(d => d.NormalizedTimestamp >= from && d.NormalizedTimestamp <= to)
                .ToList();

            var result = new WeekdayTrendResult
            {
                From = from,
                To = to,
                Unit = filtered.FirstOrDefault()?.Unit
            };

            if (filtered.Count == 0)
            {
                result.GlobalMin = 0;
                result.GlobalMax = 0;
                return result;
            }

            double globalMin = double.PositiveInfinity;
            double globalMax = double.NegativeInfinity;

            // Group by weekday index (Monday=0 … Sunday=6)
            var byWeekday = filtered.GroupBy(d => GetWeekdayIndex(d.NormalizedTimestamp));

            foreach (var weekdayGroup in byWeekday)
            {
                var dayIndex = weekdayGroup.Key;
                var dayOfWeek = IndexToDayOfWeek(dayIndex);

                // Group by calendar date, aggregate (avg)
                var points = weekdayGroup
                    .GroupBy(d => d.NormalizedTimestamp.Date)
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        var avg = g.Average(x => (double)x.Value!.Value);

                        if (avg < globalMin) globalMin = avg;
                        if (avg > globalMax) globalMax = avg;

                        return new WeekdayTrendPoint
                        {
                            Date = g.Key,
                            Value = avg,
                            SampleCount = g.Count()
                        };
                    })
                    .ToList();

                result.SeriesByDay[dayIndex] = new WeekdayTrendSeries
                {
                    Day = dayOfWeek,
                    Points = points
                };
            }

            // Avoid degenerate range
            if (double.IsInfinity(globalMin) || double.IsInfinity(globalMax))
            {
                globalMin = 0;
                globalMax = 0;
            }
            else if (globalMax == globalMin)
            {
                globalMax = globalMin + 1;
            }

            result.GlobalMin = globalMin;
            result.GlobalMax = globalMax;

            return result;
        }

        // 0 = Monday … 6 = Sunday
        private static int GetWeekdayIndex(DateTime dt)
        {
            var dow = dt.DayOfWeek;
            return dow == DayOfWeek.Sunday ? 6 : ((int)dow - 1);
        }

        private static DayOfWeek IndexToDayOfWeek(int index)
        {
            // 0->Monday(1), … 5->Saturday(6), 6->Sunday(0)
            return index == 6 ? DayOfWeek.Sunday : (DayOfWeek)(index + 1);
        }
    }
}
