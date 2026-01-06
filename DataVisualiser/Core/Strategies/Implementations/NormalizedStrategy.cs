using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Implementations;

using UnitResolutionService = UnitResolutionService;

public sealed class NormalizedStrategy : IChartComputationStrategy
{
    private readonly DateTime                      _from;
    private readonly string                        _labelLeft;
    private readonly string                        _labelRight;
    private readonly IEnumerable<MetricData> _left;
    private readonly NormalizationMode             _mode;
    private readonly IEnumerable<MetricData> _right;
    private readonly ISmoothingService             _smoothingService;
    private readonly ITimelineService              _timelineService;
    private readonly DateTime                      _to;
    private readonly IUnitResolutionService        _unitResolutionService;

    public NormalizedStrategy(IEnumerable<MetricData> left, IEnumerable<MetricData> right, string labelLeft, string labelRight, DateTime from, DateTime to) : this(left, right, labelLeft, labelRight, from, to, NormalizationMode.PercentageOfMax)
    {
    }

    public NormalizedStrategy(IEnumerable<MetricData> left, IEnumerable<MetricData> right, string labelLeft, string labelRight, DateTime from, DateTime to, NormalizationMode mode, ITimelineService? timelineService = null, ISmoothingService? smoothingService = null, IUnitResolutionService? unitResolutionService = null)
    {
        _left = left ?? Array.Empty<MetricData>();
        _right = right ?? Array.Empty<MetricData>();
        _labelLeft = labelLeft ?? "Left";
        _labelRight = labelRight ?? "Right";
        _from = from;
        _to = to;
        _mode = mode;
        _timelineService = timelineService ?? new TimelineService();
        _smoothingService = smoothingService ?? new SmoothingService();
        _unitResolutionService = unitResolutionService ?? new UnitResolutionService();
    }

    public string PrimaryLabel => $"{_labelLeft} ~ {_labelRight}";

    // For RelativeToMax we *do* want a proper label for the baseline series.
    public string SecondaryLabel => _mode == NormalizationMode.RelativeToMax ? $"{_labelRight} (baseline)" : string.Empty;

    public string? Unit { get; private set; }

    public ChartComputationResult? Compute()
    {
        var prepared = StrategyComputationHelper.PrepareDataForComputation(_left, _right, _from, _to);

        if (prepared == null)
            return null;

        var (ordered1, ordered2, dateRange, tickInterval) = prepared.Value;
        var timestamps = StrategyComputationHelper.CombineTimestamps(ordered1, ordered2);

        if (timestamps.Count == 0)
            return null;

        // Use unified timeline service
        var timeline = _timelineService.GenerateTimeline(_from, _to, timestamps);
        var intervalIndices = _timelineService.MapToIntervals(timestamps, timeline);

        // Use unified smoothing service
        var interpSmoothed1 = _smoothingService.SmoothSeries(ordered1, timestamps, _from, _to).
                                                ToList();
        var interpSmoothed2 = _smoothingService.SmoothSeries(ordered2, timestamps, _from, _to).
                                                ToList();

        var (rawValues1, rawValues2) = ExtractAlignedRawValues(ordered1, ordered2, timestamps);
        var normalization = NormalizeSeries(rawValues1, rawValues2, interpSmoothed1, interpSmoothed2);

        if (normalization == null)
            return null;

        var (raw1, raw2, smooth1, smooth2) = normalization.Value;

        Unit = _unitResolutionService.ResolveUnit(ordered1, ordered2);

        return new ChartComputationResult
        {
                Timestamps = timestamps,
                IntervalIndices = intervalIndices.ToList(),
                NormalizedIntervals = timeline.NormalizedIntervals.ToList(),
                PrimaryRawValues = raw1,
                PrimarySmoothed = smooth1.ToList(),
                SecondaryRawValues = raw2,
                SecondarySmoothed = smooth2.ToList(),
                TickInterval = timeline.TickInterval,
                DateRange = timeline.DateRange,
                Unit = Unit
        };
    }

    private static(List<double> Raw1, List<double> Raw2) ExtractAlignedRawValues(List<MetricData> ordered1, List<MetricData> ordered2, List<DateTime> timestamps)
    {
        var (dict1, dict2) = StrategyComputationHelper.CreateTimestampValueDictionaries(ordered1, ordered2);
        return StrategyComputationHelper.ExtractAlignedRawValues(timestamps, dict1, dict2);
    }

    private(List<double> Raw1, List<double> Raw2, List<double> Smoothed1, List<double> Smoothed2)? NormalizeSeries(List<double> raw1, List<double> raw2, List<double> smoothed1, List<double> smoothed2)
    {
        if (_mode != NormalizationMode.RelativeToMax)
        {
            var nRaw1 = MathHelper.ReturnValueNormalized(raw1, _mode);
            var nRaw2 = MathHelper.ReturnValueNormalized(raw2, _mode);
            var nSmooth1 = MathHelper.ReturnValueNormalized(smoothed1, _mode);
            var nSmooth2 = MathHelper.ReturnValueNormalized(smoothed2, _mode);

            if (nRaw1 == null || nRaw2 == null || nSmooth1 == null || nSmooth2 == null)
                return null;

            return (nRaw1, nRaw2, nSmooth1, nSmooth2);
        }

        // RelativeToMax mode
        var rawNorm = MathHelper.ReturnValueNormalized(raw1, raw2, _mode);
        var smoothNorm = MathHelper.ReturnValueNormalized(smoothed1, smoothed2, _mode);

        if (rawNorm.FirstNormalized == null || rawNorm.SecondNormalized == null || smoothNorm.FirstNormalized == null || smoothNorm.SecondNormalized == null)
            return null;

        return (rawNorm.FirstNormalized, rawNorm.SecondNormalized, smoothNorm.FirstNormalized, smoothNorm.SecondNormalized);
    }
}
