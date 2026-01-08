using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Implementations;

using UnitResolutionService = UnitResolutionService;

/// <summary>
///     Strategy for rendering transform operation results (unary or binary operations).
///     Uses the existing chart computation pipeline for proper normalization and axis formatting.
/// </summary>
public sealed class TransformResultStrategy : IChartComputationStrategy
{
    private readonly List<double> _computedValues;
    private readonly List<MetricData> _data;
    private readonly DateTime _from;
    private readonly ISmoothingService _smoothingService;
    private readonly ITimelineService _timelineService;
    private readonly DateTime _to;
    private readonly IUnitResolutionService _unitResolutionService;

    public TransformResultStrategy(List<MetricData> data, List<double> computedValues, string label, DateTime from, DateTime to, ITimelineService? timelineService = null, ISmoothingService? smoothingService = null, IUnitResolutionService? unitResolutionService = null)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        _computedValues = computedValues ?? throw new ArgumentNullException(nameof(computedValues));
        PrimaryLabel = label ?? "Transform Result";
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
        if (_data.Count == 0 || _computedValues.Count == 0)
            return null;

        var minCount = Math.Min(_data.Count, _computedValues.Count);
        if (minCount == 0)
            return null;

        // Extract timestamps and values
        var timestamps = new List<DateTime>(minCount);
        var rawValues = new List<double>(minCount);

        for (var i = 0; i < minCount; i++)
        {
            timestamps.Add(_data[i].NormalizedTimestamp);

            var value = _computedValues[i];
            if (double.IsNaN(value) || double.IsInfinity(value))
                rawValues.Add(double.NaN);
            else
                rawValues.Add(value);
        }

        // Use unified unit resolution service
        Unit = _unitResolutionService.ResolveUnit(_data);

        // Use unified timeline service
        var timeline = _timelineService.GenerateTimeline(_from, _to, timestamps);
        var intervalIndices = _timelineService.MapToIntervals(timestamps, timeline);

        // Convert computed values to MetricData for smoothing
        var dataForSmoothing = _data.Zip(_computedValues,
                                            (d, v) => new MetricData
                                            {
                                                    NormalizedTimestamp = d.NormalizedTimestamp,
                                                    Value = (decimal)v,
                                                    Unit = d.Unit,
                                                    Provider = d.Provider
                                            })
                                    .ToList();

        // Use unified smoothing service
        var smoothedValues = _smoothingService.SmoothSeries(dataForSmoothing, timestamps, _from, _to);

        return new ChartComputationResult
        {
                Timestamps = timestamps,
                IntervalIndices = intervalIndices.ToList(),
                NormalizedIntervals = timeline.NormalizedIntervals.ToList(),
                PrimaryRawValues = rawValues,
                PrimarySmoothed = smoothedValues.ToList(),
                TickInterval = timeline.TickInterval,
                DateRange = timeline.DateRange,
                Unit = Unit
        };
    }
}