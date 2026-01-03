using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using UnitResolutionService = DataVisualiser.Core.Services.UnitResolutionService;

namespace DataVisualiser.Core.Strategies.Implementations;

/// <summary>
///     Computes left - right on a shared index-aligned timeline.
/// </summary>
public sealed class DifferenceStrategy : IChartComputationStrategy
{
    private readonly DateTime                      _from;
    private readonly string                        _labelLeft;
    private readonly string                        _labelRight;
    private readonly IEnumerable<HealthMetricData> _left;
    private readonly IEnumerable<HealthMetricData> _right;
    private readonly ISmoothingService             _smoothingService;
    private readonly ITimelineService              _timelineService;
    private readonly DateTime                      _to;
    private readonly IUnitResolutionService        _unitResolutionService;

    public DifferenceStrategy(IEnumerable<HealthMetricData> left, IEnumerable<HealthMetricData> right, string labelLeft, string labelRight, DateTime from, DateTime to, ITimelineService? timelineService = null, ISmoothingService? smoothingService = null, IUnitResolutionService? unitResolutionService = null)
    {
        _left = left ?? Array.Empty<HealthMetricData>();
        _right = right ?? Array.Empty<HealthMetricData>();
        _labelLeft = labelLeft ?? "Left";
        _labelRight = labelRight ?? "Right";
        _from = from;
        _to = to;
        _timelineService = timelineService ?? new TimelineService();
        _smoothingService = smoothingService ?? new SmoothingService();
        _unitResolutionService = unitResolutionService ?? new UnitResolutionService();
    }

    public string  PrimaryLabel   => $"{_labelLeft} - {_labelRight}";
    public string  SecondaryLabel => string.Empty;
    public string? Unit           { get; private set; }

    public ChartComputationResult? Compute()
    {
        var leftOrdered = FilterAndOrder(_left);
        var rightOrdered = FilterAndOrder(_right);

        var count = Math.Min(leftOrdered.Count, rightOrdered.Count);
        if (count == 0)
            return null;

        var (timestamps, rawDiff) = ComputeIndexAlignedDifferences(leftOrdered, rightOrdered, count);

        // Use unified timeline service
        var timeline = _timelineService.GenerateTimeline(_from, _to, timestamps);
        var intervalIndices = _timelineService.MapToIntervals(timestamps, timeline);

        // Use unified smoothing service
        var smoothedValues = CreateSmoothedDifferenceSeries(timestamps, rawDiff, leftOrdered);

        Unit = _unitResolutionService.ResolveUnit(leftOrdered, rightOrdered);

        return new ChartComputationResult
        {
                Timestamps = timestamps,
                IntervalIndices = intervalIndices.ToList(),
                NormalizedIntervals = timeline.NormalizedIntervals.ToList(),
                PrimaryRawValues = rawDiff,
                PrimarySmoothed = smoothedValues.ToList(),
                TickInterval = timeline.TickInterval,
                DateRange = timeline.DateRange,
                Unit = Unit
        };
    }

    private List<HealthMetricData> FilterAndOrder(IEnumerable<HealthMetricData> source)
    {
        return StrategyComputationHelper.FilterAndOrderByRange(source, _from, _to);
    }

    private static(List<DateTime> Timestamps, List<double> RawDifferences) ComputeIndexAlignedDifferences(IReadOnlyList<HealthMetricData> left, IReadOnlyList<HealthMetricData> right, int count)
    {
        var timestamps = new List<DateTime>(count);
        var diffs = new List<double>(count);

        for (var i = 0; i < count; i++)
        {
            var l = left[i];
            var r = right[i];

            timestamps.Add(l.NormalizedTimestamp);

            if (!l.Value.HasValue || !r.Value.HasValue)
                diffs.Add(double.NaN);
            else
                diffs.Add((double)l.Value.Value - (double)r.Value.Value);
        }

        return (timestamps, diffs);
    }

    private IReadOnlyList<double> CreateSmoothedDifferenceSeries(IReadOnlyList<DateTime> timestamps, IReadOnlyList<double> rawDiff, IReadOnlyList<HealthMetricData> leftOrdered)
    {
        var diffData = new List<HealthMetricData>(timestamps.Count);

        for (var i = 0; i < timestamps.Count; i++)
            diffData.Add(new HealthMetricData
            {
                    NormalizedTimestamp = timestamps[i],
                    Value = double.IsNaN(rawDiff[i]) ? null : (decimal)rawDiff[i],
                    Unit = leftOrdered[i].Unit
            });

        // Use unified smoothing service
        return _smoothingService.SmoothSeries(diffData, timestamps, _from, _to);
    }

    // Unit resolution moved to IUnitResolutionService
}