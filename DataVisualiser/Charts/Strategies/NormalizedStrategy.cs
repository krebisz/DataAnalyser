namespace DataVisualiser.Charts.Strategies
{
    using DataVisualiser.Charts.Computation;
    using DataVisualiser.Helper;
    using DataVisualiser.Models;
    using DataVisualiser.Services.Abstractions;
    using DataVisualiser.Services.Implementations;

    public sealed class NormalizedStrategy : IChartComputationStrategy
    {
        private readonly IEnumerable<HealthMetricData> _left;
        private readonly IEnumerable<HealthMetricData> _right;
        private readonly DateTime _from;
        private readonly DateTime _to;
        private readonly string _labelLeft;
        private readonly string _labelRight;
        private readonly NormalizationMode _mode;
        private readonly ITimelineService _timelineService;
        private readonly ISmoothingService _smoothingService;

        public NormalizedStrategy(
            IEnumerable<HealthMetricData> left,
            IEnumerable<HealthMetricData> right,
            string labelLeft,
            string labelRight,
            DateTime from,
            DateTime to)
            : this(left, right, labelLeft, labelRight, from, to, NormalizationMode.PercentageOfMax)
        {
        }

        public NormalizedStrategy(
            IEnumerable<HealthMetricData> left,
            IEnumerable<HealthMetricData> right,
            string labelLeft,
            string labelRight,
            DateTime from,
            DateTime to,
            NormalizationMode mode,
            ITimelineService? timelineService = null,
            ISmoothingService? smoothingService = null)
        {
            _left = left ?? Array.Empty<HealthMetricData>();
            _right = right ?? Array.Empty<HealthMetricData>();
            _labelLeft = labelLeft ?? "Left";
            _labelRight = labelRight ?? "Right";
            _from = from;
            _to = to;
            _mode = mode;
            _timelineService = timelineService ?? new TimelineService();
            _smoothingService = smoothingService ?? new SmoothingService();
        }

        public string PrimaryLabel => $"{_labelLeft} ~ {_labelRight}";

        // For RelativeToMax we *do* want a proper label for the baseline series.
        public string SecondaryLabel =>
            _mode == NormalizationMode.RelativeToMax ? $"{_labelRight} (baseline)" : string.Empty;

        public string? Unit { get; private set; }

        public ChartComputationResult? Compute()
        {
            var prepared = StrategyComputationHelper.PrepareDataForComputation(_left, _right, _from, _to);

            if (prepared == null) return null;

            var (ordered1, ordered2, dateRange, tickInterval) = prepared.Value;
            var timestamps = StrategyComputationHelper.CombineTimestamps(ordered1, ordered2);

            if (timestamps.Count == 0) return null;

            // Use unified timeline service
            var timeline = _timelineService.GenerateTimeline(_from, _to, timestamps);
            var intervalIndices = _timelineService.MapToIntervals(timestamps, timeline);

            // Use unified smoothing service
            var interpSmoothed1 = _smoothingService.SmoothSeries(ordered1, timestamps, _from, _to).ToList();
            var interpSmoothed2 = _smoothingService.SmoothSeries(ordered2, timestamps, _from, _to).ToList();

            var (rawValues1, rawValues2) = ExtractAlignedRawValues(ordered1, ordered2, timestamps);
            var normalization = NormalizeSeries(rawValues1, rawValues2, interpSmoothed1, interpSmoothed2);

            if (normalization == null) return null;

            var (raw1, raw2, smooth1, smooth2) = normalization.Value;

            Unit = StrategyComputationHelper.GetUnit(ordered1, ordered2);

            return new ChartComputationResult
            {
                Timestamps = timestamps,
                IntervalIndices = intervalIndices.ToList(),
                NormalizedIntervals = timeline.NormalizedIntervals.ToList(),
                PrimaryRawValues = raw1,
                PrimarySmoothed = smooth1.ToList(),
                SecondaryRawValues = raw2,
                SecondarySmoothed = smooth2.ToList(),
                TickInterval = timeline.TickInterval,
                DateRange = timeline.DateRange,
                Unit = Unit
            };
        }

        private static (List<double> Raw1, List<double> Raw2)
        ExtractAlignedRawValues(List<HealthMetricData> ordered1, List<HealthMetricData> ordered2, List<DateTime> timestamps)
        {
            var (dict1, dict2) = StrategyComputationHelper.CreateTimestampValueDictionaries(ordered1, ordered2);
            return StrategyComputationHelper.ExtractAlignedRawValues(timestamps, dict1, dict2);
        }

        private (List<double> Raw1, List<double> Raw2, List<double> Smoothed1, List<double> Smoothed2)?
        NormalizeSeries(List<double> raw1, List<double> raw2, List<double> smoothed1, List<double> smoothed2)
        {
            if (_mode != NormalizationMode.RelativeToMax)
            {
                var nRaw1 = MathHelper.ReturnValueNormalized(raw1, _mode);
                var nRaw2 = MathHelper.ReturnValueNormalized(raw2, _mode);
                var nSmooth1 = MathHelper.ReturnValueNormalized(smoothed1, _mode);
                var nSmooth2 = MathHelper.ReturnValueNormalized(smoothed2, _mode);

                if (nRaw1 == null || nSmooth1 == null) return null;

                return (nRaw1, nRaw2, nSmooth1, nSmooth2);
            }

            // RelativeToMax mode
            var rawNorm = MathHelper.ReturnValueNormalized(raw1, raw2, _mode);
            var smoothNorm = MathHelper.ReturnValueNormalized(smoothed1, smoothed2, _mode);

            return (rawNorm.FirstNormalized, rawNorm.SecondNormalized, smoothNorm.FirstNormalized, smoothNorm.SecondNormalized);
        }
    }
}
