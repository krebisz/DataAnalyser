namespace DataVisualiser.Charts.Strategies
{
    using DataVisualiser.Charts.Computation;
    using DataVisualiser.Models;
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

            var rawResults = MathHelper.ReturnValueRatios(rawValues1, rawValues2);
            var smoothedResults = MathHelper.ReturnValueRatios(interpSmoothed1, interpSmoothed2);

            if (rawResults == null || smoothedResults == null) return null;

            var unit1 = ordered1.FirstOrDefault()?.Unit;
            var unit2 = ordered2.FirstOrDefault()?.Unit;
            Unit = (!string.IsNullOrEmpty(unit1) && !string.IsNullOrEmpty(unit2)) ? $"{unit1}/{unit2}" : null;

            return new ChartComputationResult
            {
                Timestamps = combinedTimestamps,
                IntervalIndices = intervalIndices,
                NormalizedIntervals = normalizedIntervals,
                PrimaryRawValues = rawResults,
                PrimarySmoothed = smoothedResults,
                TickInterval = tickInterval,
                DateRange = dateRange,
                Unit = Unit
            };
        }
    }
}
