using DataFileReader.Canonical;

namespace DataVisualiser.Tests.Helpers;

public class MockCmsBuilder
{
    private TimeSpan _interval = TimeSpan.FromDays(1);
    private string _metricId = "metric.test";
    private int _sampleCount = 10;
    private DateTimeOffset _startTime = DateTimeOffset.UtcNow;
    private string _unitSymbol = "kg";
    private decimal? _value = 100m;

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