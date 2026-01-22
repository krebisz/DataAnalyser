using DataVisualiser.Shared.Models;

namespace DataVisualiser.Tests.Helpers;

public class HealthMetricDataBuilder
{
    private readonly string? _provider = "Test";
    private DateTime _timestamp = DateTime.UtcNow;
    private string? _unit = "kg";
    private decimal? _value = 100m;

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

    public MetricData Build()
    {
        return new MetricData
        {
                NormalizedTimestamp = _timestamp,
                Value = _value,
                Unit = _unit,
                Provider = _provider
        };
    }

    public List<MetricData> BuildSeries(int count, TimeSpan interval)
    {
        var series = new List<MetricData>();
        var current = _timestamp;
        for (var i = 0; i < count; i++)
        {
            series.Add(new MetricData
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