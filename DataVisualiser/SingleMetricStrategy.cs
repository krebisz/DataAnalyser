namespace DataVisualiser.Charts.Strategies
{
    using DataVisualiser.Class;
    using DataVisualiser.Helper;

    public sealed class SingleMetricStrategy : IChartComputationStrategy
    {
        private readonly IEnumerable<HealthMetricData> _data;
        private readonly DateTime _from;
        private readonly DateTime _to;
        private readonly string _label;

        public SingleMetricStrategy(IEnumerable<HealthMetricData> data, string label, DateTime from, DateTime to)
        {
            _data = data ?? Array.Empty<HealthMetricData>();
            _label = label ?? "Metric";
            _from = from;
            _to = to;
        }

        public string PrimaryLabel => _label;
        public string SecondaryLabel => string.Empty;
        public string? Unit { get; private set; }

        public ChartComputationResult Compute()
        {
            var orderedData = _data.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
            if (!orderedData.Any()) return null!; // engine will treat null as no-data

            var dateRange = _to - _from;
            var tickInterval = MathHelper.DetermineTickInterval(dateRange);
            var rawTimestamps = orderedData.Select(d => d.NormalizedTimestamp).ToList();
            var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);
            var intervalIndices = rawTimestamps.Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval)).ToList();
            var smoothedData = MathHelper.CreateSmoothedData(orderedData, _from, _to);
            var smoothedValues = MathHelper.InterpolateSmoothedData(smoothedData, rawTimestamps);

            Unit = orderedData.FirstOrDefault()?.Unit;

            return new ChartComputationResult
            {
                Timestamps = rawTimestamps,
                IntervalIndices = intervalIndices,
                NormalizedIntervals = normalizedIntervals,
                PrimaryRawValues = orderedData.Select(d => d.Value.HasValue ? (double)d.Value.Value : double.NaN).ToList(),
                PrimarySmoothed = smoothedValues,
                TickInterval = tickInterval,
                DateRange = dateRange,
                Unit = Unit
            };
        }
    }
}
