// File: DataVisualiser/Charts/Strategies/CombinedMetricCmsStrategy.cs
using DataFileReader.Canonical;
using DataVisualiser.Charts.Computation;
using DataVisualiser.Helper;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;
using DataVisualiser.Services.Implementations;
using UnitResolutionService = DataVisualiser.Services.Implementations.UnitResolutionService;

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
        private readonly ITimelineService _timelineService;
        private readonly ISmoothingService _smoothingService;
        private readonly IUnitResolutionService _unitResolutionService;

        public CombinedMetricCmsStrategy(
            ICanonicalMetricSeries left,
            ICanonicalMetricSeries right,
            string labelLeft,
            string labelRight,
            DateTime from,
            DateTime to,
            ITimelineService? timelineService = null,
            ISmoothingService? smoothingService = null,
            IUnitResolutionService? unitResolutionService = null)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _right = right ?? throw new ArgumentNullException(nameof(right));
            _labelLeft = labelLeft ?? "Left";
            _labelRight = labelRight ?? "Right";
            _from = from;
            _to = to;
            _timelineService = timelineService ?? new TimelineService();
            _smoothingService = smoothingService ?? new SmoothingService();
            _unitResolutionService = unitResolutionService ?? new UnitResolutionService();
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

            // Use unified timeline service
            var timeline = _timelineService.GenerateTimeline(_from, _to, timestamps);
            var intervalIndices = _timelineService.MapToIntervals(timestamps, timeline);

            // Synthesize HealthMetricData for smoothing service
            var leftSynthetic = leftOrdered.Select(p => new HealthMetricData
            {
                NormalizedTimestamp = p.Timestamp,
                Value = p.ValueDecimal,
                Unit = _left.Unit.Symbol
            }).ToList();

            var rightSynthetic = rightOrdered.Select(p => new HealthMetricData
            {
                NormalizedTimestamp = p.Timestamp,
                Value = p.ValueDecimal,
                Unit = _right.Unit.Symbol
            }).ToList();

            // Use unified smoothing service
            var primarySmoothed = _smoothingService.SmoothSeries(leftSynthetic, timestamps, _from, _to);
            var secondarySmoothed = _smoothingService.SmoothSeries(rightSynthetic, timestamps, _from, _to);

            Unit = _unitResolutionService.ResolveUnit(_left, _right);

            return new ChartComputationResult
            {
                Timestamps = timestamps,
                IntervalIndices = intervalIndices.ToList(),
                NormalizedIntervals = timeline.NormalizedIntervals.ToList(),

                PrimaryRawValues = primaryRaw,
                PrimarySmoothed = primarySmoothed.ToList(),

                SecondaryRawValues = secondaryRaw,
                SecondarySmoothed = secondarySmoothed.ToList(),

                TickInterval = timeline.TickInterval,
                DateRange = timeline.DateRange,
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

        // Smoothing logic moved to ISmoothingService
        // Unit resolution moved to IUnitResolutionService

        private sealed record CmsPoint(DateTime Timestamp, decimal? ValueDecimal);
    }
}
