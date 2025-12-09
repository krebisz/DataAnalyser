using DataVisualiser.Charts.Computation;
using DataVisualiser.Helper;
using DataVisualiser.Models;

namespace DataVisualiser.Charts.Strategies
{
    /// <summary>
    /// Computes left - right on a shared index-aligned timeline.
    /// </summary>
    public sealed class DifferenceStrategy : IChartComputationStrategy
    {
        private readonly IEnumerable<HealthMetricData> _left;
        private readonly IEnumerable<HealthMetricData> _right;
        private readonly DateTime _from;
        private readonly DateTime _to;
        private readonly string _labelLeft;
        private readonly string _labelRight;

        public DifferenceStrategy(
            IEnumerable<HealthMetricData> left,
            IEnumerable<HealthMetricData> right,
            string labelLeft,
            string labelRight,
            DateTime from,
            DateTime to)
        {
            _left = left ?? Array.Empty<HealthMetricData>();
            _right = right ?? Array.Empty<HealthMetricData>();
            _labelLeft = labelLeft ?? "Left";
            _labelRight = labelRight ?? "Right";
            _from = from;
            _to = to;
        }

        public string PrimaryLabel => $"{_labelLeft} - {_labelRight}";
        public string SecondaryLabel => string.Empty;
        public string? Unit { get; private set; }

        public ChartComputationResult? Compute()
        {
            var leftOrdered = _left
                .Where(d => d.Value.HasValue &&
                            d.NormalizedTimestamp >= _from &&
                            d.NormalizedTimestamp <= _to)
                .OrderBy(d => d.NormalizedTimestamp)
                .ToList();

            var rightOrdered = _right
                .Where(d => d.Value.HasValue &&
                            d.NormalizedTimestamp >= _from &&
                            d.NormalizedTimestamp <= _to)
                .OrderBy(d => d.NormalizedTimestamp)
                .ToList();

            var count = Math.Min(leftOrdered.Count, rightOrdered.Count);
            if (count == 0)
                return null;

            var timestamps = new List<DateTime>(count);
            var rawDiff = new List<double>(count);

            for (int i = 0; i < count; i++)
            {
                var l = leftOrdered[i];
                var r = rightOrdered[i];

                timestamps.Add(l.NormalizedTimestamp);

                if (!l.Value.HasValue || !r.Value.HasValue)
                {
                    rawDiff.Add(double.NaN);
                }
                else
                {
                    rawDiff.Add((double)l.Value.Value - (double)r.Value.Value);
                }
            }

            var dateRange = _to - _from;
            var tickInterval = MathHelper.DetermineTickInterval(dateRange);
            var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);
            var intervalIndices = timestamps
                .Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval))
                .ToList();

            // Build a synthetic sequence for smoothing (difference points).
            var diffData = new List<HealthMetricData>();
            for (int i = 0; i < count; i++)
            {
                var value = double.IsNaN(rawDiff[i]) ? (decimal?)null : (decimal?)rawDiff[i];
                diffData.Add(new HealthMetricData
                {
                    NormalizedTimestamp = timestamps[i],
                    Value = value,
                    Unit = leftOrdered[i].Unit
                });
            }

            var smoothedPoints = MathHelper.CreateSmoothedData(diffData, _from, _to);
            var smoothedValues = MathHelper.InterpolateSmoothedData(smoothedPoints, timestamps);

            var unitLeft = leftOrdered.FirstOrDefault()?.Unit;
            var unitRight = rightOrdered.FirstOrDefault()?.Unit;
            Unit = unitLeft == unitRight ? unitLeft : unitLeft ?? unitRight;

            return new ChartComputationResult
            {
                Timestamps = timestamps,
                IntervalIndices = intervalIndices,
                NormalizedIntervals = normalizedIntervals,
                PrimaryRawValues = rawDiff,
                PrimarySmoothed = smoothedValues,
                TickInterval = tickInterval,
                DateRange = dateRange,
                Unit = Unit
            };
        }
    }
}
