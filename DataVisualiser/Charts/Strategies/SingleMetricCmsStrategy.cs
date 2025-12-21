using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVisualiser.Charts.Strategies
{
    using DataFileReader.Canonical;
    using DataVisualiser.Charts.Computation;
    using DataVisualiser.Helper;
    using DataVisualiser.Models;
    using System;
    using System.Linq;

    /// <summary>
    /// Phase 4: CMS-first Single Metric strategy.
    /// Consumes Canonical Metric Series directly.
    /// </summary>
    public sealed class SingleMetricCmsStrategy : IChartComputationStrategy
    {
        private readonly ICanonicalMetricSeries _cms;
        private readonly DateTime _from;
        private readonly DateTime _to;
        private readonly string _label;

        public SingleMetricCmsStrategy(
            ICanonicalMetricSeries cms,
            string label,
            DateTime from,
            DateTime to)
        {
            _cms = cms ?? throw new ArgumentNullException(nameof(cms));
            _label = label ?? "Metric";
            _from = from;
            _to = to;
        }

        public string PrimaryLabel => _label;
        public string SecondaryLabel => string.Empty;
        public string? Unit => _cms.Unit.Symbol;

        public ChartComputationResult? Compute()
        {
            if (_cms.Samples.Count == 0)
                return null;

            // CMS boundary normalization (explicit): DateTimeOffset -> DateTime, decimal -> double (for chart values)
            var samples = _cms.Samples
                .Where(s => s.Value.HasValue)
                .Select(s => new
                {
                    Timestamp = s.Timestamp.UtcDateTime,     // DateTimeOffset -> DateTime
                    ValueDecimal = s.Value.Value,            // decimal
                    ValueDouble = (double)s.Value.Value      // double
                })
                .Where(s => s.Timestamp >= _from && s.Timestamp <= _to)
                .OrderBy(s => s.Timestamp)
                .ToList();

            if (samples.Count == 0)
                return null;

            var dateRange = _to - _from;
            var tickInterval = MathHelper.DetermineTickInterval(dateRange);

            var timestamps = samples.Select(s => s.Timestamp).ToList();
            var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);

            var intervalIndices = timestamps
                .Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval))
                .ToList();

            // Adapt to HealthMetricData ONLY for MathHelper compatibility
            // HealthMetricData.Value is decimal? (not double?), so feed decimal.
            var syntheticData = samples
                .Select(s => new HealthMetricData
                {
                    NormalizedTimestamp = s.Timestamp,
                    Value = s.ValueDecimal,                 // ✅ decimal -> decimal?
                    Unit = _cms.Unit.Symbol
                })
                .ToList();

            var smoothedData = MathHelper.CreateSmoothedData(syntheticData, _from, _to);
            var smoothedValues = MathHelper.InterpolateSmoothedData(smoothedData, timestamps);

            return new ChartComputationResult
            {
                Timestamps = timestamps,
                IntervalIndices = intervalIndices,
                NormalizedIntervals = normalizedIntervals,
                PrimaryRawValues = samples.Select(s => s.ValueDouble).ToList(),
                PrimarySmoothed = smoothedValues,
                TickInterval = tickInterval,
                DateRange = dateRange,
                Unit = _cms.Unit.Symbol
            };
        }
    }
}


