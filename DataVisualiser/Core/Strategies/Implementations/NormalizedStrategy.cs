using DataFileReader.Canonical;
using DataVisualiser.Core.Computation.TimeSeries;
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
    private readonly DateTime _from;
    private readonly string _labelLeft;
    private readonly string _labelRight;
    private readonly IEnumerable<MetricData> _left;
    private readonly NormalizationMode _mode;
    private readonly IEnumerable<MetricData> _right;
    private readonly ISmoothingService _smoothingService;
    private readonly ITimelineService _timelineService;
    private readonly DateTime _to;
    private readonly IUnitResolutionService _unitResolutionService;

    public NormalizedStrategy(IEnumerable<MetricData> left, IEnumerable<MetricData> right, string labelLeft, string labelRight, DateTime from, DateTime to) : this(left, right, labelLeft, labelRight, from, to, NormalizationMode.PercentageOfMax)
    {
    }

    public NormalizedStrategy(ICanonicalMetricSeries left, ICanonicalMetricSeries right, string labelLeft, string labelRight, DateTime from, DateTime to, NormalizationMode mode, ITimelineService? timelineService = null, ISmoothingService? smoothingService = null, IUnitResolutionService? unitResolutionService = null)
        : this(CmsConversionHelper.ConvertSamplesToHealthMetricData(left, from, to).ToList(),
            CmsConversionHelper.ConvertSamplesToHealthMetricData(right, from, to).ToList(),
            labelLeft,
            labelRight,
            from,
            to,
            mode,
            timelineService,
            smoothingService,
            unitResolutionService)
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

    public string PrimaryLabel => _labelLeft;

    public string SecondaryLabel => _labelRight;

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
        var interpSmoothed1 = _smoothingService.SmoothSeries(ordered1, timestamps, _from, _to).ToList();
        var interpSmoothed2 = _smoothingService.SmoothSeries(ordered2, timestamps, _from, _to).ToList();

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
        var raw = TimeSeriesNormalizationKernel.NormalizeSeries([raw1, raw2], _mode);
        var smoothed = TimeSeriesNormalizationKernel.NormalizeSeries([smoothed1, smoothed2], _mode);

        return (
            raw[0].ToList(),
            raw[1].ToList(),
            smoothed[0].ToList(),
            smoothed[1].ToList());
    }
}
