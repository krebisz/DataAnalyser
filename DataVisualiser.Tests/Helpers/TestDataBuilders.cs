using DataFileReader.Canonical;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Tests.Helpers;

/// <summary>
///     Builders for creating test data for parity validation.
/// </summary>
public static class TestDataBuilders
{
    public static HealthMetricDataBuilder HealthMetricData()
    {
        return new HealthMetricDataBuilder();
    }

    public static MockCmsBuilder CanonicalMetricSeries()
    {
        return new MockCmsBuilder();
    }
}

public class HealthMetricDataBuilder
{
    private readonly string?  _provider  = "Test";
    private          DateTime _timestamp = DateTime.UtcNow;
    private          string?  _unit      = "kg";
    private          decimal? _value     = 100m;

    public HealthMetricDataBuilder WithTimestamp(DateTime timestamp)
    {
        _timestamp = timestamp;
        return this;
    }

    public HealthMetricDataBuilder WithValue(decimal? value)
    {
        _value = value;
        return this;
    }

    public HealthMetricDataBuilder WithUnit(string? unit)
    {
        _unit = unit;
        return this;
    }

    public HealthMetricData Build()
    {
        return new HealthMetricData
        {
                NormalizedTimestamp = _timestamp,
                Value = _value,
                Unit = _unit,
                Provider = _provider
        };
    }

    public List<HealthMetricData> BuildSeries(int count, TimeSpan interval)
    {
        var series = new List<HealthMetricData>();
        var current = _timestamp;
        for (var i = 0; i < count; i++)
        {
            series.Add(new HealthMetricData
            {
                    NormalizedTimestamp = current,
                    Value = _value,
                    Unit = _unit,
                    Provider = _provider
            });
            current = current.Add(interval);
        }

        return series;
    }
}

public class MockCmsBuilder
{
    private TimeSpan       _interval    = TimeSpan.FromDays(1);
    private string         _metricId    = "metric.test";
    private int            _sampleCount = 10;
    private DateTimeOffset _startTime   = DateTimeOffset.UtcNow;
    private string         _unitSymbol  = "kg";
    private decimal?       _value       = 100m;

    public MockCmsBuilder WithMetricId(string metricId)
    {
        _metricId = metricId;
        return this;
    }

    public MockCmsBuilder WithStartTime(DateTimeOffset startTime)
    {
        _startTime = startTime;
        return this;
    }

    public MockCmsBuilder WithInterval(TimeSpan interval)
    {
        _interval = interval;
        return this;
    }

    public MockCmsBuilder WithValue(decimal? value)
    {
        _value = value;
        return this;
    }

    public MockCmsBuilder WithUnit(string unitSymbol)
    {
        _unitSymbol = unitSymbol;
        return this;
    }

    public MockCmsBuilder WithSampleCount(int count)
    {
        _sampleCount = count;
        return this;
    }

    public ICanonicalMetricSeries Build()
    {
        var samples = new List<MetricSample>();
        var current = _startTime;
        for (var i = 0; i < _sampleCount; i++)
        {
            samples.Add(new MetricSample(current, _value));
            current = current.Add(_interval);
        }

        return new MockCanonicalMetricSeries
        {
                MetricId = new CanonicalMetricId(_metricId),
                Samples = samples,
                Unit = new MetricUnit(_unitSymbol, true),
                Time = new TimeSemantics(TimeRepresentation.Point, _startTime, samples.Count > 0 ? samples[^1].Timestamp : null)
        };
    }
}

internal class MockCanonicalMetricSeries : ICanonicalMetricSeries
{
    public CanonicalMetricId           MetricId   { get; init; } = default!;
    public TimeSemantics               Time       { get; init; } = default!;
    public IReadOnlyList<MetricSample> Samples    { get; init; } = Array.Empty<MetricSample>();
    public MetricUnit                  Unit       { get; init; } = default!;
    public MetricDimension             Dimension  { get; init; } = MetricDimension.Unknown;
    public MetricProvenance            Provenance { get; init; } = new("Test", "Test", "1.0");
    public MetricQuality               Quality    { get; init; } = new(DataCompleteness.Complete, ValidationStatus.Validated);
}