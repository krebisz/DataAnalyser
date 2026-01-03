using DataFileReader.Canonical;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using UnitResolutionService = DataVisualiser.Core.Services.UnitResolutionService;

namespace DataVisualiser.Core.Strategies.Implementations;

/// <summary>
///     Emits N distinct series (one per sub-metric) for rendering on the main chart.
///     No aggregation - each sub-metric is rendered as a separate line.
/// </summary>
public sealed class MultiMetricStrategy : IChartComputationStrategy
{
    private readonly DateTime                                     _from;
    private readonly IReadOnlyList<string>                        _labels;
    private readonly IReadOnlyList<IEnumerable<MetricData>> _series;
    private readonly ISmoothingService                            _smoothingService;
    private readonly ITimelineService                             _timelineService;
    private readonly DateTime                                     _to;
    private readonly string?                                      _unit;
    private readonly IUnitResolutionService                       _unitResolutionService;

    /// <summary>
    ///     Legacy constructor using MetricData collections.
    /// </summary>
    public MultiMetricStrategy(IReadOnlyList<IEnumerable<MetricData>> series, IReadOnlyList<string> labels, DateTime from, DateTime to, string? unit = null, ITimelineService? timelineService = null, ISmoothingService? smoothingService = null, IUnitResolutionService? unitResolutionService = null)
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
        _timelineService = timelineService ?? new TimelineService();
        _smoothingService = smoothingService ?? new SmoothingService();
        _unitResolutionService = unitResolutionService ?? new UnitResolutionService();
    }

    /// <summary>
    ///     Phase 4: Constructor using Canonical Metric Series.
    ///     Validates metric compatibility and converts CMS to MetricData for processing.
    /// </summary>
    public MultiMetricStrategy(IReadOnlyList<ICanonicalMetricSeries> cmsSeries, IReadOnlyList<string> labels, DateTime from, DateTime to)
    {
        if (cmsSeries == null || cmsSeries.Count == 0)
            throw new ArgumentException("At least one metric series is required.", nameof(cmsSeries));
        if (labels == null || labels.Count != cmsSeries.Count)
            throw new ArgumentException("Labels count must match series count.", nameof(labels));

        // Validate compatibility before processing
        var canonicalIds = cmsSeries.Where(cms => cms != null && cms.MetricId != null).
                                     Select(cms => cms.MetricId.Value).
                                     ToList();

        if (canonicalIds.Count != cmsSeries.Count)
            throw new ArgumentException("All CMS series must have valid metric identities.", nameof(cmsSeries));

        if (!MetricCompatibilityHelper.ValidateCompatibility(canonicalIds))
        {
            var reason = MetricCompatibilityHelper.GetIncompatibilityReason(canonicalIds);
            throw new ArgumentException($"Cannot combine incompatible metrics: {reason}", nameof(cmsSeries));
        }

        // Convert each CMS to MetricData using helper
        var series = cmsSeries.Select(cms => CmsConversionHelper.ConvertSamplesToHealthMetricData(cms, from, to).
                                                                 ToList()).
                               ToList();

        _series = series;
        _labels = labels;
        _from = from;
        _to = to;
        // Use unified unit resolution service (default implementation)
        var unitResolutionService = new UnitResolutionService();
        _unit = cmsSeries.Count > 0 ? unitResolutionService.ResolveUnit(cmsSeries[0]) : null;
        _timelineService = new TimelineService();
        _smoothingService = new SmoothingService();
        _unitResolutionService = unitResolutionService;
    }

    public string  PrimaryLabel   => _labels.Count > 0 ? _labels[0] : "Multi-Metric";
    public string  SecondaryLabel => string.Empty;
    public string? Unit           { get; private set; }

    public ChartComputationResult? Compute()
    {
        var seriesResults = ProcessAllSeries();

        if (seriesResults.Count == 0)
            return null;

        return BuildComputationResult(seriesResults);
    }

    private List<SeriesResult> ProcessAllSeries()
    {
        var results = new List<SeriesResult>();

        for (var i = 0; i < _series.Count; i++)
        {
            var processed = ProcessSingleSeries(_series[i], i, _labels[i]);
            if (processed != null)
                results.Add(processed);
        }

        return results;
    }

    /// <summary>
    ///     Processes a single series: filters, orders, applies smoothing, and builds a SeriesResult.
    ///     Returns null if the series is empty.
    /// </summary>
    private SeriesResult? ProcessSingleSeries(IEnumerable<MetricData> seriesData, int seriesIndex, string label)
    {
        var orderedData = StrategyComputationHelper.FilterAndOrderByRange(seriesData, _from, _to);
        if (orderedData.Count == 0)
            return null;

        var rawTimestamps = orderedData.Select(d => d.NormalizedTimestamp).
                                        ToList();
        var rawValues = orderedData.Select(d => d.Value.HasValue ? (double)d.Value.Value : double.NaN).
                                    ToList();

        var smoothedValues = _smoothingService.SmoothSeries(orderedData, rawTimestamps, _from, _to);

        if (Unit == null)
            Unit = _unitResolutionService.ResolveUnit(orderedData);

        return BuildSeriesResult(seriesIndex, label, rawTimestamps, rawValues, smoothedValues.ToList());
    }


    /// <summary>
    ///     Builds a SeriesResult from processed series data.
    /// </summary>
    private SeriesResult BuildSeriesResult(int seriesIndex, string label, List<DateTime> rawTimestamps, List<double> rawValues, List<double> smoothedValues)
    {
        return new SeriesResult
        {
                SeriesId = $"series_{seriesIndex}",
                DisplayName = label,
                Timestamps = rawTimestamps,
                RawValues = rawValues,
                Smoothed = smoothedValues
        };
    }

    /// <summary>
    ///     Collects all unique.timestamps across all series for the main timeline.
    /// </summary>
    private List<DateTime> CollectAllTimestamps(List<SeriesResult> seriesResults)
    {
        return seriesResults.SelectMany(s => s.Timestamps).
                             Distinct().
                             OrderBy(t => t).
                             ToList();
    }

    /// <summary>
    ///     Builds the final ChartComputationResult from processed series results.
    /// </summary>
    private ChartComputationResult BuildComputationResult(List<SeriesResult> seriesResults)
    {
        // Collect all unique timestamps across all series for the main timeline
        var allTimestamps = CollectAllTimestamps(seriesResults);

        // Use unified timeline service
        var timeline = _timelineService.GenerateTimeline(_from, _to, allTimestamps);
        var intervalIndices = _timelineService.MapToIntervals(allTimestamps, timeline);

        return new ChartComputationResult
        {
                Timestamps = allTimestamps,
                IntervalIndices = intervalIndices.ToList(),
                NormalizedIntervals = timeline.NormalizedIntervals.ToList(),
                TickInterval = timeline.TickInterval,
                DateRange = timeline.DateRange,
                Unit = Unit ?? _unit,
                // Populate Series array with one result per sub-metric
                Series = seriesResults
        };
    }
}