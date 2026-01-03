using DataFileReader.Canonical;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Implementations;

using UnitResolutionService = UnitResolutionService;

public sealed class SingleMetricStrategy : IChartComputationStrategy
{
    private readonly ICanonicalMetricSeries?        _cmsData;
    private readonly IEnumerable<MetricData>? _data;
    private readonly DateTime                       _from;
    private readonly ISmoothingService              _smoothingService;
    private readonly ITimelineService               _timelineService;
    private readonly DateTime                       _to;
    private readonly IUnitResolutionService         _unitResolutionService;
    private readonly bool                           _useCms;

    /// <summary>
    ///     Legacy constructor using MetricData.
    /// </summary>
    public SingleMetricStrategy(IEnumerable<MetricData> data, string label, DateTime from, DateTime to, ITimelineService? timelineService = null, ISmoothingService? smoothingService = null, IUnitResolutionService? unitResolutionService = null)
    {
        _data = data ?? Array.Empty<MetricData>();
        _cmsData = null;
        PrimaryLabel = label ?? "Metric";
        _from = from;
        _to = to;
        _useCms = false;
        _timelineService = timelineService ?? new TimelineService();
        _smoothingService = smoothingService ?? new SmoothingService();
        _unitResolutionService = unitResolutionService ?? new UnitResolutionService();
    }

    /// <summary>
    ///     Phase 4: Constructor using Canonical Metric Series.
    /// </summary>
    public SingleMetricStrategy(ICanonicalMetricSeries cmsData, string label, DateTime from, DateTime to, ITimelineService? timelineService = null, ISmoothingService? smoothingService = null, IUnitResolutionService? unitResolutionService = null)
    {
        _data = null;
        _cmsData = cmsData ?? throw new ArgumentNullException(nameof(cmsData));
        PrimaryLabel = label ?? "Metric";
        _from = from;
        _to = to;
        _useCms = true;
        _timelineService = timelineService ?? new TimelineService();
        _smoothingService = smoothingService ?? new SmoothingService();
        _unitResolutionService = unitResolutionService ?? new UnitResolutionService();
    }

    public string PrimaryLabel { get; }

    public string  SecondaryLabel => string.Empty;
    public string? Unit           { get; private set; }

    public ChartComputationResult? Compute()
    {
        if (_useCms && _cmsData != null)
            return ComputeFromCms();

        if (_data == null)
            return null;

        var orderedData = StrategyComputationHelper.PrepareOrderedData(_data);

        if (!orderedData.Any())
            return null; // engine will treat null as no-data

        // Use unified unit resolution service
        var unitFromData = _unitResolutionService.ResolveUnit(orderedData);
        return ComputeFromHealthMetricData(orderedData, unitFromData);
    }

    /// <summary>
    ///     Phase 4: Compute from Canonical Metric Series.
    /// </summary>
    private ChartComputationResult? ComputeFromCms()
    {
        if (_cmsData == null || _cmsData.Samples.Count == 0)
            return null;

        // Convert CMS samples to MetricData for compatibility with existing smoothing logic
        var healthMetricData = CmsConversionHelper.ConvertSamplesToHealthMetricData(_cmsData, _from, _to).
                                                   ToList();

        if (!healthMetricData.Any())
            return null;

        var orderedData = StrategyComputationHelper.PrepareOrderedData(healthMetricData);

        // Use unit from CMS (more authoritative than individual data points)
        var unitFromCms = _cmsData.Unit.Symbol;
        return ComputeFromHealthMetricData(orderedData, unitFromCms);
    }

    /// <summary>
    ///     Common computation logic for both legacy and CMS data paths.
    ///     Performs smoothing, interpolation, and chart result construction.
    /// </summary>
    /// <param name="orderedData">Filtered and ordered health metric data</param>
    /// <param name="unit">Optional unit override (if null, uses first data point's unit)</param>
    /// <returns>Chart computation result or null if no data</returns>
    private ChartComputationResult? ComputeFromHealthMetricData(IReadOnlyList<MetricData> orderedData, string? unit = null)
    {
        if (orderedData == null || orderedData.Count == 0)
            return null;

        // orderedData is already a List (from PrepareOrderedData), but CreateSmoothedData requires List<T>
        // Convert IReadOnlyList to List only if necessary
        var dataList = orderedData is List<MetricData> list ? list : orderedData.ToList();

        var rawTimestamps = dataList.Select(d => d.NormalizedTimestamp).
                                     ToList();

        // Use unified timeline service
        var timeline = _timelineService.GenerateTimeline(_from, _to, rawTimestamps);
        var intervalIndices = _timelineService.MapToIntervals(rawTimestamps, timeline);

        // Use unified smoothing service
        var smoothedValues = _smoothingService.SmoothSeries(dataList, rawTimestamps, _from, _to);

        // Use provided unit or fall back to unit resolution service
        Unit = unit ?? _unitResolutionService.ResolveUnit(dataList);

        return new ChartComputationResult
        {
                Timestamps = rawTimestamps,
                IntervalIndices = intervalIndices.ToList(),
                NormalizedIntervals = timeline.NormalizedIntervals.ToList(),
                PrimaryRawValues = dataList.Select(d => d.Value.HasValue ? (double)d.Value.Value : double.NaN).
                                            ToList(),
                PrimarySmoothed = smoothedValues.ToList(),
                TickInterval = timeline.TickInterval,
                DateRange = timeline.DateRange,
                Unit = Unit
        };
    }
}