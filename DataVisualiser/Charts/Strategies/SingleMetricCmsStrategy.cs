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
    using DataVisualiser.Services.Abstractions;
    using DataVisualiser.Services.Implementations;
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
        private readonly ITimelineService _timelineService;
        private readonly ISmoothingService _smoothingService;

        public SingleMetricCmsStrategy(
            ICanonicalMetricSeries cms,
            string label,
            DateTime from,
            DateTime to,
            ITimelineService? timelineService = null,
            ISmoothingService? smoothingService = null)
        {
            _cms = cms ?? throw new ArgumentNullException(nameof(cms));
            _label = label ?? "Metric";
            _from = from;
            _to = to;
            _timelineService = timelineService ?? new TimelineService();
            _smoothingService = smoothingService ?? new SmoothingService();
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

            var timestamps = samples.Select(s => s.Timestamp).ToList();

            // Use unified timeline service
            var timeline = _timelineService.GenerateTimeline(_from, _to, timestamps);
            var intervalIndices = _timelineService.MapToIntervals(timestamps, timeline);

            // Adapt to HealthMetricData for smoothing service compatibility
            var syntheticData = samples
                .Select(s => new HealthMetricData
                {
                    NormalizedTimestamp = s.Timestamp,
                    Value = s.ValueDecimal,
                    Unit = _cms.Unit.Symbol
                })
                .ToList();

            // Use unified smoothing service
            var smoothedValues = _smoothingService.SmoothSeries(syntheticData, timestamps, _from, _to);

            return new ChartComputationResult
            {
                Timestamps = timestamps,
                IntervalIndices = intervalIndices.ToList(),
                NormalizedIntervals = timeline.NormalizedIntervals.ToList(),
                PrimaryRawValues = samples.Select(s => s.ValueDouble).ToList(),
                PrimarySmoothed = smoothedValues.ToList(),
                TickInterval = timeline.TickInterval,
                DateRange = timeline.DateRange,
                Unit = _cms.Unit.Symbol
            };
        }
    }
}


