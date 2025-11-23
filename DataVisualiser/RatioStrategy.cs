using System;
using System.Collections.Generic;
using System.Linq;

namespace DataVisualiser.Charts.Strategies
{
    using DataVisualiser;
    using DataVisualiser.Class;
    using DataVisualiser.Helper;

    public sealed class RatioStrategy : IChartComputationStrategy
    {
        private readonly IEnumerable<HealthMetricData> _left;
        private readonly IEnumerable<HealthMetricData> _right;
        private readonly DateTime _from;
        private readonly DateTime _to;
        private readonly string _labelLeft;
        private readonly string _labelRight;

        public RatioStrategy(IEnumerable<HealthMetricData> left, IEnumerable<HealthMetricData> right, string labelLeft, string labelRight, DateTime from, DateTime to)
        {
            _left = left ?? Array.Empty<HealthMetricData>();
            _right = right ?? Array.Empty<HealthMetricData>();
            _labelLeft = labelLeft ?? "L";
            _labelRight = labelRight ?? "R";
            _from = from;
            _to = to;
        }

        public string PrimaryLabel => $"{_labelLeft} / {_labelRight}";
        public string SecondaryLabel => string.Empty;
        public string? Unit { get; private set; }

        public ChartComputationResult Compute()
        {
            var ordered1 = _left.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
            var ordered2 = _right.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();

            if (!ordered1.Any() && !ordered2.Any()) return null!;

            var dateRange = _to - _from;
            var tickInterval = MathHelper.DetermineTickInterval(dateRange);

            var combinedTimestamps = ordered1.Select(d => d.NormalizedTimestamp)
                .Concat(ordered2.Select(d => d.NormalizedTimestamp))
                .Distinct()
                .OrderBy(dt => dt)
                .ToList();

            if (!combinedTimestamps.Any()) return null!;

            var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);
            var intervalIndices = combinedTimestamps.Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval)).ToList();

            var smoothed1 = MathHelper.CreateSmoothedData(ordered1, _from, _to);
            var smoothed2 = MathHelper.CreateSmoothedData(ordered2, _from, _to);
            var interpSmoothed1 = MathHelper.InterpolateSmoothedData(smoothed1, combinedTimestamps);
            var interpSmoothed2 = MathHelper.InterpolateSmoothedData(smoothed2, combinedTimestamps);

            var dict1 = ordered1.GroupBy(d => d.NormalizedTimestamp).ToDictionary(g => g.Key, g => (double)g.First().Value!.Value);
            var dict2 = ordered2.GroupBy(d => d.NormalizedTimestamp).ToDictionary(g => g.Key, g => (double)g.First().Value!.Value);

            var rawValues1 = combinedTimestamps.Select(ts => dict1.TryGetValue(ts, out var v1) ? v1 : double.NaN).ToList();
            var rawValues2 = combinedTimestamps.Select(ts => dict2.TryGetValue(ts, out var v2) ? v2 : double.NaN).ToList();

            var rawRatios = MathHelper.ReturnValueRatios(rawValues1, rawValues2);
            var smoothedRatios = MathHelper.ReturnValueRatios(interpSmoothed1, interpSmoothed2);

            var unit1 = ordered1.FirstOrDefault()?.Unit;
            var unit2 = ordered2.FirstOrDefault()?.Unit;
            Unit = (!string.IsNullOrEmpty(unit1) && !string.IsNullOrEmpty(unit2)) ? $"{unit1}/{unit2}" : null;

            return new ChartComputationResult
            {
                Timestamps = combinedTimestamps,
                IntervalIndices = intervalIndices,
                NormalizedIntervals = normalizedIntervals,
                PrimaryRawValues = rawRatios,
                PrimarySmoothed = smoothedRatios,
                TickInterval = tickInterval,
                DateRange = dateRange,
                Unit = Unit
            };
        }
    }
}
