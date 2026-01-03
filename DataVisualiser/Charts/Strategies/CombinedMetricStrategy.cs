using DataVisualiser.Charts.Computation;
using DataVisualiser.Helper;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;
using DataVisualiser.Services.Implementations;
using UnitResolutionService = DataVisualiser.Services.Implementations.UnitResolutionService;

namespace DataVisualiser.Charts.Strategies;

/// <summary>
///     Renders two metrics on the same canonical timeline.
///     Alignment is by ordered index (after date filtering) to avoid
///     timestamp precision mismatches killing the secondary series.
/// </summary>
public sealed class CombinedMetricStrategy : IChartComputationStrategy
{
    private readonly DateTime _from;
    private readonly IEnumerable<HealthMetricData> _left;
    private readonly IEnumerable<HealthMetricData> _right;
    private readonly ISmoothingService _smoothingService;
    private readonly ITimelineService _timelineService;
    private readonly DateTime _to;
    private readonly IUnitResolutionService _unitResolutionService;

    public CombinedMetricStrategy(IEnumerable<HealthMetricData> left, IEnumerable<HealthMetricData> right, string labelLeft, string labelRight, DateTime from, DateTime to, ITimelineService? timelineService = null, ISmoothingService? smoothingService = null, IUnitResolutionService? unitResolutionService = null)
    {
        _left = left ?? Array.Empty<HealthMetricData>();
        _right = right ?? Array.Empty<HealthMetricData>();
        PrimaryLabel = labelLeft ?? "Left";
        SecondaryLabel = labelRight ?? "Right";
        _from = from;
        _to = to;
        _timelineService = timelineService ?? new TimelineService();
        _smoothingService = smoothingService ?? new SmoothingService();
        _unitResolutionService = unitResolutionService ?? new UnitResolutionService();
    }

    public string PrimaryLabel { get; }

    public string SecondaryLabel { get; }

    public string? Unit { get; private set; }

    public ChartComputationResult? Compute()
    {
        var leftOrdered = FilterAndOrder(_left);
        var rightOrdered = FilterAndOrder(_right);

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

    private List<HealthMetricData> FilterAndOrder(IEnumerable<HealthMetricData> source)
    {
        return StrategyComputationHelper.FilterAndOrderByRange(source, _from, _to);
    }

    // Alignment logic moved to StrategyComputationHelper
    // Smoothing logic moved to ISmoothingService
    // Unit resolution moved to IUnitResolutionService
}