namespace DataVisualiser.Charts.Strategies
{
    using DataVisualiser.Charts.Computation;
    using DataVisualiser.Helper;
    using DataVisualiser.Models;
    using System.Linq;

    /// <summary>
    /// Strategy for rendering transform operation results (unary or binary operations).
    /// Uses the existing chart computation pipeline for proper normalization and axis formatting.
    /// </summary>
    public sealed class TransformResultStrategy : IChartComputationStrategy
    {
        private readonly List<HealthMetricData> _data;
        private readonly List<double> _computedValues;
        private readonly DateTime _from;
        private readonly DateTime _to;
        private readonly string _label;

        public TransformResultStrategy(
            List<HealthMetricData> data,
            List<double> computedValues,
            string label,
            DateTime from,
            DateTime to)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _computedValues = computedValues ?? throw new ArgumentNullException(nameof(computedValues));
            _label = label ?? "Transform Result";
            _from = from;
            _to = to;
        }

        public string PrimaryLabel => _label;
        public string SecondaryLabel => string.Empty;
        public string? Unit { get; private set; }

        public ChartComputationResult? Compute()
        {
            if (_data.Count == 0 || _computedValues.Count == 0)
                return null;

            var minCount = Math.Min(_data.Count, _computedValues.Count);
            if (minCount == 0)
                return null;

            // Extract timestamps and values
            var timestamps = new List<DateTime>(minCount);
            var rawValues = new List<double>(minCount);

            for (int i = 0; i < minCount; i++)
            {
                timestamps.Add(_data[i].NormalizedTimestamp);

                var value = _computedValues[i];
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    rawValues.Add(double.NaN);
                }
                else
                {
                    rawValues.Add(value);
                }
            }

            // Use unit from first data point if available
            Unit = _data.FirstOrDefault()?.Unit;

            // Determine tick interval based on date range
            var dateRange = _to - _from;
            var tickInterval = MathHelper.DetermineTickInterval(dateRange);

            // Generate normalized intervals for proper axis spacing
            var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);

            // Map timestamps to interval indices
            var intervalIndices = timestamps
                .Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval))
                .ToList();

            // Apply smoothing to transform results for consistency with other charts
            var smoothedData = MathHelper.CreateSmoothedData(
                _data.Zip(_computedValues, (d, v) => new HealthMetricData
                {
                    NormalizedTimestamp = d.NormalizedTimestamp,
                    Value = (decimal)v,
                    Unit = d.Unit,
                    Provider = d.Provider
                }).ToList(),
                _from,
                _to);

            var smoothedValues = MathHelper.InterpolateSmoothedData(smoothedData, timestamps);

            return new ChartComputationResult
            {
                Timestamps = timestamps,
                IntervalIndices = intervalIndices,
                NormalizedIntervals = normalizedIntervals,
                PrimaryRawValues = rawValues,
                PrimarySmoothed = smoothedValues,
                TickInterval = tickInterval,
                DateRange = dateRange,
                Unit = Unit
            };
        }
    }
}
