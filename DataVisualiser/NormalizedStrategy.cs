namespace DataVisualiser.Charts.Strategies
{
    using DataVisualiser.Class;
    using DataVisualiser.Helper;

    public sealed class NormalizedStrategy : IChartComputationStrategy
    {
        private readonly IEnumerable<HealthMetricData> _left;
        private readonly IEnumerable<HealthMetricData> _right;
        private readonly DateTime _from;
        private readonly DateTime _to;
        private readonly string _labelLeft;
        private readonly string _labelRight;
        private readonly NormalizationMode _mode = NormalizationMode.PercentageOfMax;

        public NormalizedStrategy(IEnumerable<HealthMetricData> left, IEnumerable<HealthMetricData> right, string labelLeft, string labelRight, DateTime from, DateTime to)
        {
            _left = left ?? Array.Empty<HealthMetricData>();
            _right = right ?? Array.Empty<HealthMetricData>();
            _labelLeft = labelLeft ?? "Left";
            _labelRight = labelRight ?? "Right";
            _from = from;
            _to = to;
        }


        public NormalizedStrategy(IEnumerable<HealthMetricData> left, IEnumerable<HealthMetricData> right, string labelLeft, string labelRight, DateTime from, DateTime to, NormalizationMode mode)
        {
            _left = left ?? Array.Empty<HealthMetricData>();
            _right = right ?? Array.Empty<HealthMetricData>();
            _labelLeft = labelLeft ?? "Left";
            _labelRight = labelRight ?? "Right";
            _from = from;
            _to = to;
            _mode = mode;
        }

        public string PrimaryLabel => $"{_labelLeft} ~ {_labelRight}";
        public string SecondaryLabel => string.Empty;
        public string? Unit { get; private set; }

        public ChartComputationResult? Compute()
        {
            var prepared = StrategyComputationHelper.PrepareDataForComputation(_left, _right, _from, _to);
            if (prepared == null) return null;

            var (ordered1, ordered2, dateRange, tickInterval) = prepared.Value;
            var combinedTimestamps = StrategyComputationHelper.CombineTimestamps(ordered1, ordered2);
            if (!combinedTimestamps.Any()) return null;

            var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);
            var intervalIndices = combinedTimestamps
                .Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval))
                .ToList();

            var (interpSmoothed1, interpSmoothed2) = StrategyComputationHelper.ProcessSmoothedData(
                ordered1, ordered2, combinedTimestamps, _from, _to);

            var (dict1, dict2) = StrategyComputationHelper.CreateTimestampValueDictionaries(ordered1, ordered2);
            var (rawValues1, rawValues2) = StrategyComputationHelper.ExtractAlignedRawValues(
                combinedTimestamps, dict1, dict2);

            List<double>? rawResults1;
            List<double>? rawResults2;
            List<double>? smoothedResults1;
            List<double>? smoothedResults2;

            if (_mode != NormalizationMode.RelativeToMax)
            {
                rawResults1 = MathHelper.ReturnValueNormalized(rawValues1, NormalizationMode.ZeroToOne);
                rawResults2 = MathHelper.ReturnValueNormalized(rawValues2, NormalizationMode.ZeroToOne);
                smoothedResults1 = MathHelper.ReturnValueNormalized(interpSmoothed1, NormalizationMode.ZeroToOne);
                smoothedResults2 = MathHelper.ReturnValueNormalized(interpSmoothed2, NormalizationMode.ZeroToOne);
            }
            else
            {
                var rawResults = MathHelper.ReturnValueNormalized(rawValues1, rawValues2, _mode);
                rawResults1 = rawResults.FirstNormalized;
                rawResults2 = rawResults.SecondNormalized;

                var smoothResults = MathHelper.ReturnValueNormalized(interpSmoothed1, interpSmoothed2, _mode);
                smoothedResults1 = smoothResults.FirstNormalized;
                smoothedResults2 = smoothResults.SecondNormalized;
            }

            if (rawResults1 == null || smoothedResults1 == null) return null;

            Unit = StrategyComputationHelper.GetUnit(ordered1, ordered2);

            return new ChartComputationResult
            {
                Timestamps = combinedTimestamps,
                IntervalIndices = intervalIndices,
                NormalizedIntervals = normalizedIntervals,
                PrimaryRawValues = rawResults1,
                PrimarySmoothed = smoothedResults1,
                SecondaryRawValues = rawResults2,
                SecondarySmoothed = smoothedResults2,
                TickInterval = tickInterval,
                DateRange = dateRange,
                Unit = Unit
            };
        }
    }
}
