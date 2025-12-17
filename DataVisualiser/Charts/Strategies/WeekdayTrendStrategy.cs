using System;
using System.Collections.Generic;
using System.Linq;
using DataVisualiser.Helper;
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

            var filtered = FilterData(data, from, to);

            var result = new WeekdayTrendResult
            {
                From = from,
                To = to,
                Unit = filtered.FirstOrDefault()?.Unit
            };

            if (filtered.Count == 0)
            {
                SetDegenerateRange(result);
                return result;
            }

            var (seriesByDay, globalMin, globalMax) =
                BuildWeekdaySeries(filtered);

            foreach (var kvp in seriesByDay)
            {
                result.SeriesByDay[kvp.Key] = kvp.Value;
            }

            NormalizeGlobalRange(result, globalMin, globalMax);

            return result;
        }

        private static List<HealthMetricData> FilterData(IEnumerable<HealthMetricData> data, DateTime from, DateTime to)
        {
            return StrategyComputationHelper.FilterAndOrderByRange(data, from, to);
        }

        private (Dictionary<int, WeekdayTrendSeries> SeriesByDay, double GlobalMin, double GlobalMax)
        BuildWeekdaySeries(List<HealthMetricData> filtered)
        {
            var seriesByDay = new Dictionary<int, WeekdayTrendSeries>();

            double globalMin = double.PositiveInfinity;
            double globalMax = double.NegativeInfinity;

            var byWeekday = filtered.GroupBy(d => GetWeekdayIndex(d.NormalizedTimestamp));

            foreach (var weekdayGroup in byWeekday)
            {
                var dayIndex = weekdayGroup.Key;
                var dayOfWeek = IndexToDayOfWeek(dayIndex);

                var points = weekdayGroup
                    .GroupBy(d => d.NormalizedTimestamp.Date)
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        var avg = g.Average(x => (double)x.Value!.Value);

                        globalMin = Math.Min(globalMin, avg);
                        globalMax = Math.Max(globalMax, avg);

                        return new WeekdayTrendPoint
                        {
                            Date = g.Key,
                            Value = avg,
                            SampleCount = g.Count()
                        };
                    })
                    .ToList();

                seriesByDay[dayIndex] = new WeekdayTrendSeries
                {
                    Day = dayOfWeek,
                    Points = points
                };
            }

            return (seriesByDay, globalMin, globalMax);
        }

        private static void NormalizeGlobalRange(WeekdayTrendResult result, double globalMin, double globalMax)
        {
            if (double.IsInfinity(globalMin) ||
                double.IsInfinity(globalMax))
            {
                SetDegenerateRange(result);
                return;
            }

            if (globalMax == globalMin)
            {
                globalMax = globalMin + 1;
            }

            result.GlobalMin = globalMin;
            result.GlobalMax = globalMax;
        }

        private static void SetDegenerateRange(WeekdayTrendResult result)
        {
            result.GlobalMin = 0;
            result.GlobalMax = 0;
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
