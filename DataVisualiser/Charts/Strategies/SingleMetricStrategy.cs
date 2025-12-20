namespace DataVisualiser.Charts.Strategies
{
    using DataFileReader.Canonical;
    using DataVisualiser.Charts.Computation;
    using DataVisualiser.Helper;
    using DataVisualiser.Models;
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

            var orderedData = PrepareOrderedData(_data);

            if (!orderedData.Any())
                return null; // engine will treat null as no-data

            // Use unit from first data point (legacy behavior)
            var unitFromData = orderedData.FirstOrDefault()?.Unit;
            return ComputeFromHealthMetricData(orderedData, unitFromData);
        }

        /// <summary>
        /// Phase 4: Compute from Canonical Metric Series.
        /// </summary>
        private ChartComputationResult? ComputeFromCms()
        {
            if (_cmsData == null || _cmsData.Samples.Count == 0)
                return null;

            // Convert CMS samples to HealthMetricData for compatibility with existing smoothing logic
            var healthMetricData = CmsConversionHelper.ConvertSamplesToHealthMetricData(
                _cmsData,
                _from,
                _to)
                .ToList();

            if (!healthMetricData.Any())
                return null;

            var orderedData = PrepareOrderedData(healthMetricData);

            // Use unit from CMS (more authoritative than individual data points)
            var unitFromCms = _cmsData.Unit.Symbol;
            return ComputeFromHealthMetricData(orderedData, unitFromCms);
        }

        /// <summary>
        /// Filters out null-valued points and orders data by timestamp.
        /// Shared between legacy and CMS paths to ensure consistent behavior.
        /// </summary>
        private static List<HealthMetricData> PrepareOrderedData(IEnumerable<HealthMetricData> source)
        {
            return source
                .Where(d => d.Value.HasValue)
                .OrderBy(d => d.NormalizedTimestamp)
                .ToList();
        }

        /// <summary>
        /// Common computation logic for both legacy and CMS data paths.
        /// Performs smoothing, interpolation, and chart result construction.
        /// </summary>
        /// <param name="orderedData">Filtered and ordered health metric data</param>
        /// <param name="unit">Optional unit override (if null, uses first data point's unit)</param>
        /// <returns>Chart computation result or null if no data</returns>
        private ChartComputationResult? ComputeFromHealthMetricData(
            IReadOnlyList<HealthMetricData> orderedData,
            string? unit = null)
        {
            if (orderedData == null || orderedData.Count == 0)
                return null;

            // Convert to List for MathHelper methods that require List<T>
            var dataList = orderedData.ToList();

            var dateRange = _to - _from;
            var tickInterval = MathHelper.DetermineTickInterval(dateRange);
            var rawTimestamps = dataList.Select(d => d.NormalizedTimestamp).ToList();
            var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);
            var intervalIndices = rawTimestamps
                .Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval))
                .ToList();
            var smoothedData = MathHelper.CreateSmoothedData(dataList, _from, _to);
            var smoothedValues = MathHelper.InterpolateSmoothedData(smoothedData, rawTimestamps);

            // Use provided unit or fall back to first data point's unit
            Unit = unit ?? dataList.FirstOrDefault()?.Unit;

            return new ChartComputationResult
            {
                Timestamps = rawTimestamps,
                IntervalIndices = intervalIndices,
                NormalizedIntervals = normalizedIntervals,
                PrimaryRawValues = dataList
                    .Select(d => d.Value.HasValue ? (double)d.Value.Value : double.NaN)
                    .ToList(),
                PrimarySmoothed = smoothedValues,
                TickInterval = tickInterval,
                DateRange = dateRange,
                Unit = Unit
            };
        }
    }
}
