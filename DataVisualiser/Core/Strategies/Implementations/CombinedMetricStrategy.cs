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
///     Renders two metrics on the same canonical timeline.
///     Alignment is by ordered index (after date filtering) to avoid
///     timestamp precision mismatches killing the secondary series.
///     Supports both CMS and Legacy data sources.
/// </summary>
public sealed class CombinedMetricStrategy : IChartComputationStrategy
{
    private readonly DateTime                       _from;
    private readonly IEnumerable<MetricData>? _left;
    private readonly ICanonicalMetricSeries?        _leftCms;
    private readonly IEnumerable<MetricData>? _right;
    private readonly ICanonicalMetricSeries?        _rightCms;
    private readonly ISmoothingService              _smoothingService;
    private readonly ITimelineService               _timelineService;
    private readonly DateTime                       _to;
    private readonly IUnitResolutionService         _unitResolutionService;
    private readonly bool                           _useCms;

    /// <summary>
    ///     Legacy constructor using MetricData.
    /// </summary>
    public CombinedMetricStrategy(IEnumerable<MetricData> left, IEnumerable<MetricData> right, string labelLeft, string labelRight, DateTime from, DateTime to, ITimelineService? timelineService = null, ISmoothingService? smoothingService = null, IUnitResolutionService? unitResolutionService = null)
    {
        _left = left ?? Array.Empty<MetricData>();
        _right = right ?? Array.Empty<MetricData>();
        _leftCms = null;
        _rightCms = null;
        PrimaryLabel = labelLeft ?? "Left";
        SecondaryLabel = labelRight ?? "Right";
        _from = from;
        _to = to;
        _useCms = false;
        _timelineService = timelineService ?? new TimelineService();
        _smoothingService = smoothingService ?? new SmoothingService();
        _unitResolutionService = unitResolutionService ?? new UnitResolutionService();
    }

    /// <summary>
    ///     CMS constructor using Canonical Metric Series.
    /// </summary>
    public CombinedMetricStrategy(ICanonicalMetricSeries left, ICanonicalMetricSeries right, string labelLeft, string labelRight, DateTime from, DateTime to, ITimelineService? timelineService = null, ISmoothingService? smoothingService = null, IUnitResolutionService? unitResolutionService = null)
    {
        _leftCms = left ?? throw new ArgumentNullException(nameof(left));
        _rightCms = right ?? throw new ArgumentNullException(nameof(right));
        _left = null;
        _right = null;
        PrimaryLabel = labelLeft ?? "Left";
        SecondaryLabel = labelRight ?? "Right";
        _from = from;
        _to = to;
        _useCms = true;
        _timelineService = timelineService ?? new TimelineService();
        _smoothingService = smoothingService ?? new SmoothingService();
        _unitResolutionService = unitResolutionService ?? new UnitResolutionService();
    }

    public string PrimaryLabel { get; }

    public string SecondaryLabel { get; }

    public string? Unit { get; private set; }

    public ChartComputationResult? Compute()
    {
        if (_useCms && _leftCms != null && _rightCms != null)
            return ComputeFromCms();

        if (_left == null || _right == null)
            return null;

        return ComputeFromHealthMetricData(_left, _right);
    }

    /// <summary>
    ///     Compute from Canonical Metric Series.
    /// </summary>
    private ChartComputationResult? ComputeFromCms()
    {
        if (_leftCms == null || _rightCms == null)
            return null;

        if (_leftCms.Samples.Count == 0 && _rightCms.Samples.Count == 0)
            return null;

        var leftOrdered = FilterAndOrderCms(_leftCms);
        var rightOrdered = FilterAndOrderCms(_rightCms);

        if (leftOrdered.Count == 0 && rightOrdered.Count == 0)
            return null;

        var count = Math.Min(leftOrdered.Count, rightOrdered.Count);
        if (count == 0)
            return null;

        var (timestamps, primaryRaw, secondaryRaw) = AlignSeriesCms(leftOrdered, rightOrdered, count);

        if (timestamps.Count == 0)
            return null;

        var timeline = _timelineService.GenerateTimeline(_from, _to, timestamps);
        var intervalIndices = _timelineService.MapToIntervals(timestamps, timeline);

        var (primarySmoothed, secondarySmoothed) = SmoothSeriesCms(leftOrdered, rightOrdered, timestamps);

        Unit = _unitResolutionService.ResolveUnit(_leftCms, _rightCms);

        return new ChartComputationResult
        {
                Timestamps = timestamps,
                IntervalIndices = intervalIndices.ToList(),
                NormalizedIntervals = timeline.NormalizedIntervals.ToList(),
                PrimaryRawValues = primaryRaw,
                PrimarySmoothed = primarySmoothed,
                SecondaryRawValues = secondaryRaw,
                SecondarySmoothed = secondarySmoothed,
                TickInterval = timeline.TickInterval,
                DateRange = timeline.DateRange,
                Unit = Unit
        };
    }

    /// <summary>
    ///     Compute from MetricData (Legacy).
    /// </summary>
    private ChartComputationResult? ComputeFromHealthMetricData(IEnumerable<MetricData> left, IEnumerable<MetricData> right)
    {
        var leftOrdered = FilterAndOrder(left);
        var rightOrdered = FilterAndOrder(right);

        if (leftOrdered.Count == 0 && rightOrdered.Count == 0)
            return null;

        var count = Math.Min(leftOrdered.Count, rightOrdered.Count);
        if (count == 0)
            return null;

        var (timestamps, primaryRaw, secondaryRaw) = StrategyComputationHelper.AlignByIndex(leftOrdered, rightOrdered, count);

        // Use unified timeline service
        var timeline = _timelineService.GenerateTimeline(_from, _to, timestamps);
        var intervalIndices = _timelineService.MapToIntervals(timestamps, timeline);

        // Use unified smoothing service
        var primarySmoothed = _smoothingService.SmoothSeries(leftOrdered, timestamps, _from, _to);
        var secondarySmoothed = _smoothingService.SmoothSeries(rightOrdered, timestamps, _from, _to);

        Unit = _unitResolutionService.ResolveUnit(leftOrdered, rightOrdered);

        return new ChartComputationResult
        {
                Timestamps = timestamps,
                IntervalIndices = intervalIndices.ToList(),
                NormalizedIntervals = timeline.NormalizedIntervals.ToList(),
                PrimaryRawValues = primaryRaw,
                PrimarySmoothed = primarySmoothed.ToList(),
                SecondaryRawValues = secondaryRaw,
                SecondarySmoothed = secondarySmoothed.ToList(),
                TickInterval = timeline.TickInterval,
                DateRange = timeline.DateRange,
                Unit = Unit
        };
    }

    private List<MetricData> FilterAndOrder(IEnumerable<MetricData> source)
    {
        return StrategyComputationHelper.FilterAndOrderByRange(source, _from, _to);
    }

    private List<CmsPoint> FilterAndOrderCms(ICanonicalMetricSeries cms)
    {
        return cms.Samples.Where(s => s.Value.HasValue).
                   Select(s => new CmsPoint(s.Timestamp.UtcDateTime, s.Value!.Value)).
                   Where(p => p.Timestamp >= _from && p.Timestamp <= _to).
                   OrderBy(p => p.Timestamp).
                   ToList();
    }

    private(List<DateTime> Timestamps, List<double> Primary, List<double> Secondary) AlignSeriesCms(List<CmsPoint> left, List<CmsPoint> right, int count)
    {
        var leftTuples = left.Select(p => (p.Timestamp, (decimal?)p.ValueDecimal)).
                              ToList();
        var rightTuples = right.Select(p => (p.Timestamp, (decimal?)p.ValueDecimal)).
                                ToList();

        var (timestamps, primaryRaw, secondaryRaw) = StrategyComputationHelper.AlignByIndex(leftTuples, rightTuples, count);

        return (timestamps, primaryRaw.ToList(), secondaryRaw.ToList());
    }

    private(List<double> Primary, List<double> Secondary) SmoothSeriesCms(List<CmsPoint> left, List<CmsPoint> right, List<DateTime> timestamps)
    {
        var leftSynthetic = left.Select(p => new MetricData
                                 {
                                         NormalizedTimestamp = p.Timestamp,
                                         Value = p.ValueDecimal,
                                         Unit = _leftCms!.Unit.Symbol
                                 }).
                                 ToList();

        var rightSynthetic = right.Select(p => new MetricData
                                   {
                                           NormalizedTimestamp = p.Timestamp,
                                           Value = p.ValueDecimal,
                                           Unit = _rightCms!.Unit.Symbol
                                   }).
                                   ToList();

        var primarySmoothed = _smoothingService.SmoothSeries(leftSynthetic, timestamps, _from, _to);
        var secondarySmoothed = _smoothingService.SmoothSeries(rightSynthetic, timestamps, _from, _to);

        return (primarySmoothed.ToList(), secondarySmoothed.ToList());
    }

    private sealed record CmsPoint(DateTime Timestamp, decimal ValueDecimal);

    // Alignment logic moved to StrategyComputationHelper
    // Smoothing logic moved to ISmoothingService
    // Unit resolution moved to IUnitResolutionService
}
