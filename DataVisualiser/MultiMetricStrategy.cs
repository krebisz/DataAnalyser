using DataVisualiser.Charts;
using DataVisualiser.Charts.Computation;
using DataVisualiser.Models;
using DataVisualiser.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataVisualiser.Charts.Strategies
{
    /// <summary>
    /// Emits N distinct series (one per sub-metric) for rendering on the main chart.
    /// No aggregation - each sub-metric is rendered as a separate line.
    /// </summary>
    public sealed class MultiMetricStrategy : IChartComputationStrategy
    {
        private readonly IReadOnlyList<IEnumerable<HealthMetricData>> _series;
        private readonly IReadOnlyList<string> _labels;
        private readonly DateTime _from;
        private readonly DateTime _to;
        private readonly string? _unit;

        public MultiMetricStrategy(
            IReadOnlyList<IEnumerable<HealthMetricData>> series,
            IReadOnlyList<string> labels,
            DateTime from,
            DateTime to,
            string? unit = null)
        {
            if (series == null || series.Count == 0)
                throw new ArgumentException("At least one metric series is required.", nameof(series));
            if (labels == null || labels.Count != series.Count)
                throw new ArgumentException("Labels count must match series count.", nameof(labels));

            _series = series;
            _labels = labels;
            _from = from;
            _to = to;
            _unit = unit;
        }

        public string PrimaryLabel => _labels.Count > 0 ? _labels[0] : "Multi-Metric";
        public string SecondaryLabel => string.Empty;
        public string? Unit { get; private set; }

        public ChartComputationResult? Compute()
        {
            var seriesResults = new List<SeriesResult>();

            // Process each series independently (like SingleMetricStrategy)
            for (int i = 0; i < _series.Count; i++)
            {
                var seriesData = _series[i];
                var label = _labels[i];

                // Filter, order, and process like SingleMetricStrategy
                var orderedData = seriesData
                    .Where(d => d.Value.HasValue &&
                               d.NormalizedTimestamp >= _from &&
                               d.NormalizedTimestamp <= _to)
                    .OrderBy(d => d.NormalizedTimestamp)
                    .ToList();

                if (!orderedData.Any())
                    continue; // Skip empty series

                var rawTimestamps = orderedData.Select(d => d.NormalizedTimestamp).ToList();
                var rawValues = orderedData
                    .Select(d => d.Value.HasValue ? (double)d.Value.Value : double.NaN)
                    .ToList();

                // Apply smoothing (same as SingleMetricStrategy)
                var smoothedData = MathHelper.CreateSmoothedData(orderedData, _from, _to);
                var smoothedValues = MathHelper.InterpolateSmoothedData(smoothedData, rawTimestamps);

                // Capture unit from first non-null series
                if (Unit == null)
                {
                    Unit = orderedData.FirstOrDefault()?.Unit;
                }

                seriesResults.Add(new SeriesResult
                {
                    SeriesId = $"series_{i}",
                    DisplayName = label,
                    Timestamps = rawTimestamps,
                    RawValues = rawValues,
                    Smoothed = smoothedValues
                });
            }

            if (seriesResults.Count == 0)
                return null;

            // Determine common metadata from all series
            var dateRange = _to - _from;
            var tickInterval = MathHelper.DetermineTickInterval(dateRange);
            var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);

            // Collect all unique timestamps across all series for the main timeline
            var allTimestamps = seriesResults
                .SelectMany(s => s.Timestamps)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var intervalIndices = allTimestamps
                .Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval))
                .ToList();

            return new ChartComputationResult
            {
                Timestamps = allTimestamps,
                IntervalIndices = intervalIndices,
                NormalizedIntervals = normalizedIntervals,
                TickInterval = tickInterval,
                DateRange = dateRange,
                Unit = Unit ?? _unit,
                // Populate Series array with one result per sub-metric
                Series = seriesResults
            };
        }
    }
}
