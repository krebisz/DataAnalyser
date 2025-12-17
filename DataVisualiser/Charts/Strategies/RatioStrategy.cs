using DataVisualiser.Charts.Computation;
using DataVisualiser.Helper;
using DataVisualiser.Models;

namespace DataVisualiser.Charts.Strategies
{
    /// <summary>
    /// Computes left / right on a shared index-aligned timeline.
    /// </summary>
    public sealed class RatioStrategy : IChartComputationStrategy
    {
        private readonly IEnumerable<HealthMetricData> _left;
        private readonly IEnumerable<HealthMetricData> _right;
        private readonly DateTime _from;
        private readonly DateTime _to;
        private readonly string _labelLeft;
        private readonly string _labelRight;

        public RatioStrategy(
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

        public string PrimaryLabel => $"{_labelLeft} / {_labelRight}";
        public string SecondaryLabel => string.Empty;
        public string? Unit { get; private set; }

        public ChartComputationResult? Compute()
        {
            var leftOrdered = FilterAndOrder(_left);
            var rightOrdered = FilterAndOrder(_right);

            var count = Math.Min(leftOrdered.Count, rightOrdered.Count);
            if (count == 0)
                return null;

            var (timestamps, rawRatio) =
                ComputeIndexAlignedRatios(leftOrdered, rightOrdered, count);

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

            var smoothedValues =
                CreateSmoothedRatioSeries(timestamps, rawRatio);

            Unit = ResolveRatioUnit(leftOrdered, rightOrdered);

            return new ChartComputationResult
            {
                Timestamps = timestamps,
                IntervalIndices = intervalIndices,
                NormalizedIntervals = normalizedIntervals,
                PrimaryRawValues = rawRatio,
                PrimarySmoothed = smoothedValues,
                TickInterval = tickInterval,
                DateRange = dateRange,
                Unit = Unit
            };
        }

        private List<HealthMetricData> FilterAndOrder(IEnumerable<HealthMetricData> source)
        {
            return StrategyComputationHelper.FilterAndOrderByRange(source, _from, _to);
        }

        private static (List<DateTime> Timestamps, List<double> RawRatios)
        ComputeIndexAlignedRatios(IReadOnlyList<HealthMetricData> left, IReadOnlyList<HealthMetricData> right, int count)
        {
            var timestamps = new List<DateTime>(count);
            var ratios = new List<double>(count);

            for (int i = 0; i < count; i++)
            {
                var l = left[i];
                var r = right[i];

                timestamps.Add(l.NormalizedTimestamp);

                if (!l.Value.HasValue || !r.Value.HasValue || r.Value.Value == 0m)
                {
                    ratios.Add(double.NaN);
                }
                else
                {
                    ratios.Add((double)l.Value.Value / (double)r.Value.Value);
                }
            }

            return (timestamps, ratios);
        }

        private List<double> CreateSmoothedRatioSeries(IReadOnlyList<DateTime> timestamps, IReadOnlyList<double> rawRatios)
        {
            var ratioData = new List<HealthMetricData>(timestamps.Count);

            for (int i = 0; i < timestamps.Count; i++)
            {
                ratioData.Add(new HealthMetricData
                {
                    NormalizedTimestamp = timestamps[i],
                    Value = double.IsNaN(rawRatios[i])
                        ? (decimal?)null
                        : (decimal)rawRatios[i],
                    Unit = null
                });
            }

            var smoothedPoints = MathHelper.CreateSmoothedData(ratioData, _from, _to);

            return MathHelper.InterpolateSmoothedData(smoothedPoints, timestamps.ToList());
        }

        private static string? ResolveRatioUnit(IReadOnlyList<HealthMetricData> left, IReadOnlyList<HealthMetricData> right)
        {
            var unitLeft = left.FirstOrDefault()?.Unit;
            var unitRight = right.FirstOrDefault()?.Unit;

            return (!string.IsNullOrEmpty(unitLeft) && !string.IsNullOrEmpty(unitRight)) ? $"{unitLeft}/{unitRight}" : null;
        }
    }
}
