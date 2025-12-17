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
            var leftOrdered = FilterAndOrder(_left);
            var rightOrdered = FilterAndOrder(_right);

            if (leftOrdered.Count == 0 && rightOrdered.Count == 0)
                return null;

            var count = Math.Min(leftOrdered.Count, rightOrdered.Count);
            if (count == 0)
                return null;

            var (timestamps, primaryRaw, secondaryRaw) =
                AlignByIndex(leftOrdered, rightOrdered, count);

            var dateRange = _to - _from;
            var tickInterval = MathHelper.DetermineTickInterval(dateRange);
            var normalizedIntervals =
                MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);

            var intervalIndices = timestamps
                .Select(ts =>
                    MathHelper.MapTimestampToIntervalIndex(
                        ts,
                        normalizedIntervals,
                        tickInterval))
                .ToList();

            var primarySmoothed =
                CreateSmoothedSeries(leftOrdered, timestamps);

            var secondarySmoothed =
                CreateSmoothedSeries(rightOrdered, timestamps);

            Unit = ResolveUnit(leftOrdered, rightOrdered);

            return new ChartComputationResult
            {
                Timestamps = timestamps,
                IntervalIndices = intervalIndices,
                NormalizedIntervals = normalizedIntervals,
                PrimaryRawValues = primaryRaw,
                PrimarySmoothed = primarySmoothed,
                SecondaryRawValues = secondaryRaw,
                SecondarySmoothed = secondarySmoothed,
                TickInterval = tickInterval,
                DateRange = dateRange,
                Unit = Unit
            };
        }

        private List<HealthMetricData> FilterAndOrder(IEnumerable<HealthMetricData> source)
        {
            return StrategyComputationHelper.FilterAndOrderByRange(source, _from, _to);
        }

        private static (List<DateTime> Timestamps, List<double> Primary, List<double> Secondary)
        AlignByIndex(IReadOnlyList<HealthMetricData> left, IReadOnlyList<HealthMetricData> right, int count)
        {
            var timestamps = new List<DateTime>(count);
            var primary = new List<double>(count);
            var secondary = new List<double>(count);

            for (int i = 0; i < count; i++)
            {
                var l = left[i];
                var r = right[i];

                timestamps.Add(l.NormalizedTimestamp);
                primary.Add(l.Value.HasValue ? (double)l.Value.Value : double.NaN);
                secondary.Add(r.Value.HasValue ? (double)r.Value.Value : double.NaN);
            }

            return (timestamps, primary, secondary);
        }

        private List<double> CreateSmoothedSeries(List<HealthMetricData> orderedData, List<DateTime> timestamps)
        {
            var smoothedPoints =
                MathHelper.CreateSmoothedData(orderedData, _from, _to);

            return MathHelper.InterpolateSmoothedData(
                smoothedPoints,
                timestamps);
        }

        private static string? ResolveUnit(IReadOnlyList<HealthMetricData> left, IReadOnlyList<HealthMetricData> right)
        {
            var leftUnit = left.FirstOrDefault()?.Unit;
            var rightUnit = right.FirstOrDefault()?.Unit;

            return leftUnit == rightUnit ? leftUnit : leftUnit ?? rightUnit;
        }

    }
}
