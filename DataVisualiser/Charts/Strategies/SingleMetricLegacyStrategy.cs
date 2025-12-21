using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVisualiser.Charts.Strategies
{
    using DataVisualiser.Charts.Computation;
    using DataVisualiser.Helper;
    using DataVisualiser.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Legacy Single Metric strategy.
    /// Frozen in Phase 4. No new logic permitted.
    /// </summary>
    public sealed class SingleMetricLegacyStrategy : IChartComputationStrategy
    {
        private readonly IReadOnlyList<HealthMetricData> _data;
        private readonly DateTime _from;
        private readonly DateTime _to;
        private readonly string _label;

        public SingleMetricLegacyStrategy(
            IEnumerable<HealthMetricData> data,
            string label,
            DateTime from,
            DateTime to)
        {
            _data = (data ?? Array.Empty<HealthMetricData>()).ToList();
            _label = label ?? "Metric";
            _from = from;
            _to = to;
        }

        public string PrimaryLabel => _label;
        public string SecondaryLabel => string.Empty;
        public string? Unit { get; private set; }

        public ChartComputationResult? Compute()
        {
            var orderedData = _data
                .Where(d => d.Value.HasValue)
                .OrderBy(d => d.NormalizedTimestamp)
                .ToList();

            if (orderedData.Count == 0)
                return null;

            var dateRange = _to - _from;
            var tickInterval = MathHelper.DetermineTickInterval(dateRange);

            var timestamps = orderedData.Select(d => d.NormalizedTimestamp).ToList();
            var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);
            var intervalIndices = timestamps
                .Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval))
                .ToList();

            var smoothedData = MathHelper.CreateSmoothedData(orderedData, _from, _to);
            var smoothedValues = MathHelper.InterpolateSmoothedData(smoothedData, timestamps);

            Unit = orderedData.FirstOrDefault()?.Unit;

            return new ChartComputationResult
            {
                Timestamps = timestamps,
                IntervalIndices = intervalIndices,
                NormalizedIntervals = normalizedIntervals,
                PrimaryRawValues = orderedData
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

