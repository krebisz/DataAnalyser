using DataVisualiser.Charts.Computation;
using DataVisualiser.Helper;
using DataVisualiser.Models;

namespace DataVisualiser.Charts.Strategies
{
    /// <summary>
    /// Renders two metrics on the same canonical timeline.
    /// Alignment is by ordered index (after date filtering) to avoid
    /// timestamp precision mismatches killing the secondary series.
    /// </summary>
    public sealed class CombinedMetricStrategy : IChartComputationStrategy
    {
        private readonly IEnumerable<HealthMetricData> _left;
        private readonly IEnumerable<HealthMetricData> _right;
        private readonly DateTime _from;
        private readonly DateTime _to;
        private readonly string _labelLeft;
        private readonly string _labelRight;

        public CombinedMetricStrategy(
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

        public string PrimaryLabel => _labelLeft;
        public string SecondaryLabel => _labelRight;
        public string? Unit { get; private set; }

        public ChartComputationResult? Compute()
        {
            // Filter & order
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

            if (!leftOrdered.Any() && !rightOrdered.Any())
                return null;

            // Align by index – not by timestamp equality.
            var count = Math.Min(leftOrdered.Count, rightOrdered.Count);
            if (count == 0)
                return null;

            var timestamps = new List<DateTime>(count);
            var primaryRaw = new List<double>(count);
            var secondaryRaw = new List<double>(count);

            for (int i = 0; i < count; i++)
            {
                var l = leftOrdered[i];
                var r = rightOrdered[i];

                timestamps.Add(l.NormalizedTimestamp);
                primaryRaw.Add(l.Value.HasValue ? (double)l.Value.Value : double.NaN);
                secondaryRaw.Add(r.Value.HasValue ? (double)r.Value.Value : double.NaN);
            }

            var dateRange = _to - _from;
            var tickInterval = MathHelper.DetermineTickInterval(dateRange);
            var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);
            var intervalIndices = timestamps
                .Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval))
                .ToList();

            // Use existing smoothing pipeline so raw vs smoothed differ.
            var smoothedLeftPoints = MathHelper.CreateSmoothedData(leftOrdered, _from, _to);
            var smoothedLeft = MathHelper.InterpolateSmoothedData(smoothedLeftPoints, timestamps);

            var smoothedRightPoints = MathHelper.CreateSmoothedData(rightOrdered, _from, _to);
            var smoothedRight = MathHelper.InterpolateSmoothedData(smoothedRightPoints, timestamps);

            var unitLeft = leftOrdered.FirstOrDefault()?.Unit;
            var unitRight = rightOrdered.FirstOrDefault()?.Unit;
            Unit = unitLeft == unitRight ? unitLeft : unitLeft ?? unitRight;

            return new ChartComputationResult
            {
                Timestamps = timestamps,
                IntervalIndices = intervalIndices,
                NormalizedIntervals = normalizedIntervals,
                PrimaryRawValues = primaryRaw,
                PrimarySmoothed = smoothedLeft,
                SecondaryRawValues = secondaryRaw,
                SecondarySmoothed = smoothedRight,
                TickInterval = tickInterval,
                DateRange = dateRange,
                Unit = Unit
            };
        }
    }
}
