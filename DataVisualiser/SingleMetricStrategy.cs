namespace DataVisualiser.Charts.Strategies
{
    using DataVisualiser.Charts.Computation;
    using DataVisualiser.Models;
    using DataVisualiser.Helper;
    using DataFileReader.Canonical;
    using System.Linq;

    public sealed class SingleMetricStrategy : IChartComputationStrategy
    {
        private readonly IEnumerable<HealthMetricData>? _data;
        private readonly ICanonicalMetricSeries? _cmsData;
        private readonly DateTime _from;
        private readonly DateTime _to;
        private readonly string _label;
        private readonly bool _useCms;

        /// <summary>
        /// Legacy constructor using HealthMetricData.
        /// </summary>
        public SingleMetricStrategy(IEnumerable<HealthMetricData> data, string label, DateTime from, DateTime to)
        {
            _data = data ?? Array.Empty<HealthMetricData>();
            _cmsData = null;
            _label = label ?? "Metric";
            _from = from;
            _to = to;
            _useCms = false;
        }

        /// <summary>
        /// Phase 4: Constructor using Canonical Metric Series.
        /// </summary>
        public SingleMetricStrategy(ICanonicalMetricSeries cmsData, string label, DateTime from, DateTime to)
        {
            _data = null;
            _cmsData = cmsData ?? throw new ArgumentNullException(nameof(cmsData));
            _label = label ?? "Metric";
            _from = from;
            _to = to;
            _useCms = true;
        }

        public string PrimaryLabel => _label;
        public string SecondaryLabel => string.Empty;
        public string? Unit { get; private set; }

        public ChartComputationResult? Compute()
        {
            if (_useCms && _cmsData != null)
            {
                return ComputeFromCms();
            }

            if (_data == null) return null;

            var orderedData = _data.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
            if (!orderedData.Any()) return null; // engine will treat null as no-data

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

        /// <summary>
        /// Phase 4: Compute from Canonical Metric Series.
        /// </summary>
        private ChartComputationResult? ComputeFromCms()
        {
            if (_cmsData == null || _cmsData.Samples.Count == 0)
                return null;

            // Convert CMS samples to HealthMetricData for compatibility with existing smoothing logic
            var healthMetricData = _cmsData.Samples
                .Where(s => s.Value.HasValue && s.Timestamp.DateTime >= _from && s.Timestamp.DateTime <= _to)
                .Select(s => new HealthMetricData
                {
                    NormalizedTimestamp = s.Timestamp.DateTime,
                    Value = s.Value,
                    Unit = _cmsData.Unit.Symbol,
                    Provider = _cmsData.Provenance.SourceProvider
                })
                .OrderBy(d => d.NormalizedTimestamp)
                .ToList();

            if (!healthMetricData.Any())
                return null;

            var orderedData = healthMetricData.Where(d => d.Value.HasValue).ToList();
            var dateRange = _to - _from;
            var tickInterval = MathHelper.DetermineTickInterval(dateRange);
            var rawTimestamps = orderedData.Select(d => d.NormalizedTimestamp).ToList();
            var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);
            var intervalIndices = rawTimestamps.Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval)).ToList();
            var smoothedData = MathHelper.CreateSmoothedData(orderedData, _from, _to);
            var smoothedValues = MathHelper.InterpolateSmoothedData(smoothedData, rawTimestamps);

            Unit = _cmsData.Unit.Symbol;

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
