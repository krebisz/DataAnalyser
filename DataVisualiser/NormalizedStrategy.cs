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

        public ChartComputationResult Compute()
        {
            var ordered1 = _left.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
            var ordered2 = _right.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();

            if (!ordered1.Any() && !ordered2.Any()) return null!;

            var dateRange = _to - _from;
            var tickInterval = MathHelper.DetermineTickInterval(dateRange);

            var combinedTimestamps = ordered1.Select(d => d.NormalizedTimestamp)
                .Concat(ordered2.Select(d => d.NormalizedTimestamp))
                .Distinct()
                .OrderBy(dt => dt)
                .ToList();

            if (!combinedTimestamps.Any()) return null!;

            var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);
            var intervalIndices = combinedTimestamps.Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval)).ToList();

            var smoothed1 = MathHelper.CreateSmoothedData(ordered1, _from, _to);
            var smoothed2 = MathHelper.CreateSmoothedData(ordered2, _from, _to);
            var interpSmoothed1 = MathHelper.InterpolateSmoothedData(smoothed1, combinedTimestamps);
            var interpSmoothed2 = MathHelper.InterpolateSmoothedData(smoothed2, combinedTimestamps);

            var dict1 = ordered1.GroupBy(d => d.NormalizedTimestamp).ToDictionary(g => g.Key, g => (double)g.First().Value!.Value);
            var dict2 = ordered2.GroupBy(d => d.NormalizedTimestamp).ToDictionary(g => g.Key, g => (double)g.First().Value!.Value);

            var rawValues1 = combinedTimestamps.Select(ts => dict1.TryGetValue(ts, out var v1) ? v1 : double.NaN).ToList();
            var rawValues2 = combinedTimestamps.Select(ts => dict2.TryGetValue(ts, out var v2) ? v2 : double.NaN).ToList();


            List<double> rawResults1 = null!;
            List<double> rawResults12 = null!;

            List<double> smoothedResults1 = null!;
            List<double> smoothedResults2 = null!;


            if (_mode != NormalizationMode.RelativeToMax)
            {
                rawResults1 = MathHelper.ReturnValueNormalized(rawValues1, NormalizationMode.ZeroToOne);
                rawResults12 = MathHelper.ReturnValueNormalized(rawValues2, NormalizationMode.ZeroToOne);
                smoothedResults1 = MathHelper.ReturnValueNormalized(interpSmoothed1, NormalizationMode.ZeroToOne);
                smoothedResults2 = MathHelper.ReturnValueNormalized(interpSmoothed2, NormalizationMode.ZeroToOne);
            }
            else
            {
                (List<double>? rawResults1, List<double>? rawResults2) rawResults = MathHelper.ReturnValueNormalized(rawValues1, rawValues2, _mode);
                rawResults1 = rawResults.rawResults1;
                rawResults12 = rawResults.rawResults2;

                (List<double>? smoothResults1, List<double>? smoothResults2) smoothResults = MathHelper.ReturnValueNormalized(interpSmoothed1, interpSmoothed2, _mode);
                smoothedResults1 = smoothResults.smoothResults1;
                smoothedResults2 = smoothResults.smoothResults2;
            }

            Unit = ordered1.FirstOrDefault()?.Unit ?? ordered2.FirstOrDefault()?.Unit;

            return new ChartComputationResult
            {
                Timestamps = combinedTimestamps,
                IntervalIndices = intervalIndices,
                NormalizedIntervals = normalizedIntervals,
                PrimaryRawValues = rawResults1,
                PrimarySmoothed = smoothedResults1,
                SecondaryRawValues = rawResults12,
                SecondarySmoothed = smoothedResults2,
                TickInterval = tickInterval,
                DateRange = dateRange,
                Unit = Unit
            };
        }
    }
}
