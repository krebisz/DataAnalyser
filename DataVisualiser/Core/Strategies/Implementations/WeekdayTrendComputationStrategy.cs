using DataFileReader.Canonical;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Implementations;

public sealed class WeekdayTrendComputationStrategy : IChartComputationStrategy, IWeekdayTrendResultProvider
{
    private readonly ICanonicalMetricSeries? _cmsData;
    private readonly DateTime _from;
    private readonly IEnumerable<MetricData>? _legacyData;
    private readonly DateTime _to;

    public WeekdayTrendComputationStrategy(IEnumerable<MetricData> data, string label, DateTime from, DateTime to)
    {
        _legacyData = data ?? Array.Empty<MetricData>();
        _from = from;
        _to = to;
        PrimaryLabel = label ?? "Metric";
    }

    public WeekdayTrendComputationStrategy(ICanonicalMetricSeries cmsData, string label, DateTime from, DateTime to)
    {
        _cmsData = cmsData ?? throw new ArgumentNullException(nameof(cmsData));
        _from = from;
        _to = to;
        PrimaryLabel = label ?? "Metric";
    }

    public string PrimaryLabel { get; }

    public string SecondaryLabel => string.Empty;

    public string? Unit { get; private set; }

    public ChartComputationResult? Compute()
    {
        var data = _cmsData != null ? CmsConversionHelper.ConvertSamplesToHealthMetricData(_cmsData, _from, _to) : _legacyData ?? Array.Empty<MetricData>();

        var strategy = new WeekdayTrendStrategy();
        ExtendedResult = strategy.Compute(data, _from, _to);
        Unit = ExtendedResult?.Unit;

        if (ExtendedResult == null)
            return null;

        return new ChartComputationResult
        {
                Timestamps = new List<DateTime>(),
                IntervalIndices = new List<int>(),
                NormalizedIntervals = new List<DateTime>(),
                PrimaryRawValues = new List<double>(),
                PrimarySmoothed = new List<double>(),
                TickInterval = TickInterval.Day,
                DateRange = _to - _from,
                Unit = Unit
        };
    }

    public WeekdayTrendResult? ExtendedResult { get; private set; }
}