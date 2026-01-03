using DataVisualiser.Charts.Computation;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;
using DataVisualiser.Services.Implementations;

namespace DataVisualiser.Charts.Strategies;

using UnitResolutionService = UnitResolutionService;

/// <summary>
///     Legacy Single Metric strategy.
///     Frozen in Phase 4. No new logic permitted.
/// </summary>
public sealed class SingleMetricLegacyStrategy : IChartComputationStrategy
{
    private readonly IReadOnlyList<HealthMetricData> _data;
    private readonly DateTime _from;
    private readonly ISmoothingService _smoothingService;
    private readonly ITimelineService _timelineService;
    private readonly DateTime _to;
    private readonly IUnitResolutionService _unitResolutionService;

    public SingleMetricLegacyStrategy(IEnumerable<HealthMetricData> data, string label, DateTime from, DateTime to, ITimelineService? timelineService = null, ISmoothingService? smoothingService = null, IUnitResolutionService? unitResolutionService = null)
    {
        _data = (data ?? Array.Empty<HealthMetricData>()).ToList();
        PrimaryLabel = label ?? "Metric";
        _from = from;
        _to = to;
        _timelineService = timelineService ?? new TimelineService();
        _smoothingService = smoothingService ?? new SmoothingService();
        _unitResolutionService = unitResolutionService ?? new UnitResolutionService();
    }

    public string PrimaryLabel { get; }

    public string SecondaryLabel => string.Empty;
    public string? Unit { get; private set; }

    public ChartComputationResult? Compute()
    {
        var orderedData = _data.Where(d => d.Value.HasValue).
            OrderBy(d => d.NormalizedTimestamp).
            ToList();

        if (orderedData.Count == 0)
            return null;

        var timestamps = orderedData.Select(d => d.NormalizedTimestamp).
            ToList();

        // Use unified timeline service
        var timeline = _timelineService.GenerateTimeline(_from, _to, timestamps);
        var intervalIndices = _timelineService.MapToIntervals(timestamps, timeline);

        // Use unified smoothing service
        var smoothedValues = _smoothingService.SmoothSeries(orderedData, timestamps, _from, _to);

        Unit = _unitResolutionService.ResolveUnit(orderedData);

        return new ChartComputationResult
        {
            Timestamps = timestamps,
            IntervalIndices = intervalIndices.ToList(),
            NormalizedIntervals = timeline.NormalizedIntervals.ToList(),
            PrimaryRawValues = orderedData.Select(d => d.Value.HasValue ? (double)d.Value.Value : double.NaN).
                ToList(),
            PrimarySmoothed = smoothedValues.ToList(),
            TickInterval = timeline.TickInterval,
            DateRange = timeline.DateRange,
            Unit = Unit
        };
    }
}