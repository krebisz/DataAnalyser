using DataVisualiser.Charts;
using DataVisualiser.Charts.Computation;
using DataVisualiser.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataVisualiser.Charts.Strategies
{
    /// <summary>
    /// Aggregates multiple sub-metric series into a single primary series
    /// for rendering on the main chart.
    ///
    /// This strategy is intentionally constrained to fit the existing
    /// dual-series ChartComputationResult contract.
    /// </summary>
    public sealed class MultiMetricStrategy : IChartComputationStrategy
    {
        private readonly IReadOnlyList<IEnumerable<HealthMetricData>> _series;
        private readonly string _label;
        private readonly string? _unit;

        public MultiMetricStrategy(
            IReadOnlyList<IEnumerable<HealthMetricData>> series,
            string label,
            string? unit)
        {
            if (series == null || series.Count == 0)
                throw new ArgumentException("At least one metric series is required.", nameof(series));

            _series = series;
            _label = label;
            _unit = unit;
        }

        public string PrimaryLabel => _label;
        public string SecondaryLabel => string.Empty;
        public string? Unit => _unit;

        public ChartComputationResult? Compute()
        {
            // Flatten and group by normalized timestamp
            var grouped = _series
                .SelectMany(s => s)
                .Where(d => d.Value.HasValue)
                .GroupBy(d => d.NormalizedTimestamp)
                .OrderBy(g => g.Key)
                .ToList();

            if (grouped.Count == 0)
                return null;

            var timestamps = new List<DateTime>();
            var rawValues = new List<double>();

            foreach (var group in grouped)
            {
                var values = group
                    .Select(d => (double)d.Value!.Value)
                    .ToList();

                if (values.Count == 0)
                    continue;

                timestamps.Add(group.Key);

                // Simple aggregation: mean
                rawValues.Add(values.Average());
            }

            if (timestamps.Count == 0)
                return null;

            return new ChartComputationResult
            {
                Timestamps = timestamps,
                PrimaryRawValues = rawValues,
                PrimarySmoothed = rawValues, // smoothing handled downstream if enabled
                Unit = _unit
            };
        }
    }
}
