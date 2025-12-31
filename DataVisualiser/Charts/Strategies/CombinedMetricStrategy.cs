using DataVisualiser.Charts.Computation;
using DataVisualiser.Helper;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;
using DataVisualiser.Services.Implementations;

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
        private readonly ITimelineService _timelineService;
        private readonly ISmoothingService _smoothingService;

        public CombinedMetricStrategy(
            IEnumerable<HealthMetricData> left,
            IEnumerable<HealthMetricData> right,
            string labelLeft,
            string labelRight,
            DateTime from,
            DateTime to,
            ITimelineService? timelineService = null,
            ISmoothingService? smoothingService = null)
        {
            _left = left ?? Array.Empty<HealthMetricData>();
            _right = right ?? Array.Empty<HealthMetricData>();
            _labelLeft = labelLeft ?? "Left";
            _labelRight = labelRight ?? "Right";
            _from = from;
            _to = to;
            _timelineService = timelineService ?? new TimelineService();
            _smoothingService = smoothingService ?? new SmoothingService();
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

            // Use unified timeline service
            var timeline = _timelineService.GenerateTimeline(_from, _to, timestamps);
            var intervalIndices = _timelineService.MapToIntervals(timestamps, timeline);

            // Use unified smoothing service
            var primarySmoothed = _smoothingService.SmoothSeries(leftOrdered, timestamps, _from, _to);
            var secondarySmoothed = _smoothingService.SmoothSeries(rightOrdered, timestamps, _from, _to);

            Unit = ResolveUnit(leftOrdered, rightOrdered);

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

        // Smoothing logic moved to ISmoothingService

        private static string? ResolveUnit(IReadOnlyList<HealthMetricData> left, IReadOnlyList<HealthMetricData> right)
        {
            var leftUnit = left.FirstOrDefault()?.Unit;
            var rightUnit = right.FirstOrDefault()?.Unit;

            return leftUnit == rightUnit ? leftUnit : leftUnit ?? rightUnit;
        }

    }
}
