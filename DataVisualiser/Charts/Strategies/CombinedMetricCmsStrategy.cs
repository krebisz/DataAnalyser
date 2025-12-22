// File: DataVisualiser/Charts/Strategies/CombinedMetricCmsStrategy.cs
using DataFileReader.Canonical;
using DataVisualiser.Charts.Computation;
using DataVisualiser.Helper;
using DataVisualiser.Models;

namespace DataVisualiser.Charts.Strategies
{
    /// <summary>
    /// Phase 4: CMS-first Combined Metric strategy.
    /// Renders two canonical metrics on the same timeline.
    ///
    /// IMPORTANT: alignment is by ordered index (after date filtering),
    /// mirroring legacy CombinedMetricStrategy to avoid timestamp precision mismatches.
    /// </summary>
    public sealed class CombinedMetricCmsStrategy : IChartComputationStrategy
    {
        private readonly ICanonicalMetricSeries _left;
        private readonly ICanonicalMetricSeries _right;
        private readonly DateTime _from;
        private readonly DateTime _to;
        private readonly string _labelLeft;
        private readonly string _labelRight;

        public CombinedMetricCmsStrategy(
            ICanonicalMetricSeries left,
            ICanonicalMetricSeries right,
            string labelLeft,
            string labelRight,
            DateTime from,
            DateTime to)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _right = right ?? throw new ArgumentNullException(nameof(right));
            _labelLeft = labelLeft ?? "Left";
            _labelRight = labelRight ?? "Right";
            _from = from;
            _to = to;
        }

        public string PrimaryLabel => _labelLeft;
        public string SecondaryLabel => _labelRight;

        // Prefer the left unit; if missing, use right. If both present and disagree, keep left (legacy-ish behavior).
        public string? Unit { get; private set; }

        public ChartComputationResult? Compute()
        {
            if (_left.Samples.Count == 0 && _right.Samples.Count == 0)
                return null;

            // Filter + order inside [from,to] (DateTimeOffset -> DateTime boundary normalization)
            var leftOrdered = FilterAndOrder(_left, _from, _to);
            var rightOrdered = FilterAndOrder(_right, _from, _to);

            if (leftOrdered.Count == 0 && rightOrdered.Count == 0)
                return null;

            var count = Math.Min(leftOrdered.Count, rightOrdered.Count);
            if (count == 0)
                return null;

            var (timestamps, primaryRaw, secondaryRaw) = AlignByIndex(leftOrdered, rightOrdered, count);

            var dateRange = _to - _from;
            var tickInterval = MathHelper.DetermineTickInterval(dateRange);
            var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);

            var intervalIndices = timestamps
                .Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval))
                .ToList();

            // Smoothing uses existing MathHelper pipeline => synthesize HealthMetricData
            var leftSynthetic = leftOrdered.Select(p => new HealthMetricData
            {
                NormalizedTimestamp = p.Timestamp,
                Value = p.ValueDecimal, // decimal? -> decimal?
                Unit = _left.Unit.Symbol
            }).ToList();

            var rightSynthetic = rightOrdered.Select(p => new HealthMetricData
            {
                NormalizedTimestamp = p.Timestamp,
                Value = p.ValueDecimal,
                Unit = _right.Unit.Symbol
            }).ToList();

            var primarySmoothed = CreateSmoothedSeries(leftSynthetic, timestamps);
            var secondarySmoothed = CreateSmoothedSeries(rightSynthetic, timestamps);

            Unit = ResolveUnit(_left, _right);

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

        // ----- helpers -----

        private static List<CmsPoint> FilterAndOrder(ICanonicalMetricSeries cms, DateTime from, DateTime to)
        {
            return cms.Samples
                .Where(s => s.Value.HasValue)
                .Select(s => new CmsPoint(
                    Timestamp: s.Timestamp.UtcDateTime, // DateTimeOffset -> DateTime
                    ValueDecimal: s.Value               // decimal?
                ))
                .Where(p => p.Timestamp >= from && p.Timestamp <= to)
                .OrderBy(p => p.Timestamp)
                .ToList();
        }

        private static (List<DateTime> Timestamps, List<double> Primary, List<double> Secondary)
            AlignByIndex(IReadOnlyList<CmsPoint> left, IReadOnlyList<CmsPoint> right, int count)
        {
            var timestamps = new List<DateTime>(count);
            var primary = new List<double>(count);
            var secondary = new List<double>(count);

            for (int i = 0; i < count; i++)
            {
                var l = left[i];
                var r = right[i];

                timestamps.Add(l.Timestamp);

                primary.Add(l.ValueDecimal.HasValue ? (double)l.ValueDecimal.Value : double.NaN);
                secondary.Add(r.ValueDecimal.HasValue ? (double)r.ValueDecimal.Value : double.NaN);
            }

            return (timestamps, primary, secondary);
        }

        private List<double> CreateSmoothedSeries(List<HealthMetricData> orderedData, List<DateTime> timestamps)
        {
            var smoothedPoints = MathHelper.CreateSmoothedData(orderedData, _from, _to);
            return MathHelper.InterpolateSmoothedData(smoothedPoints, timestamps);
        }

        private static string? ResolveUnit(ICanonicalMetricSeries left, ICanonicalMetricSeries right)
        {
            var leftUnit = left.Unit.Symbol;
            var rightUnit = right.Unit.Symbol;

            if (string.IsNullOrWhiteSpace(leftUnit)) return string.IsNullOrWhiteSpace(rightUnit) ? null : rightUnit;
            return leftUnit; // authoritative (even if mismatch)
        }

        private sealed record CmsPoint(DateTime Timestamp, decimal? ValueDecimal);
    }
}
