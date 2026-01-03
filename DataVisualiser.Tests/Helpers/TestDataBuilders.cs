using DataFileReader.Canonical;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Tests.Helpers
{
    /// <summary>
    /// Builders for creating test data for parity validation.
    /// </summary>
    public static class TestDataBuilders
    {
        public static HealthMetricDataBuilder HealthMetricData() => new();
        public static MockCmsBuilder CanonicalMetricSeries() => new();
    }

    public class HealthMetricDataBuilder
    {
        private DateTime _timestamp = DateTime.UtcNow;
        private decimal? _value = 100m;
        private string? _unit = "kg";
        private string? _provider = "Test";

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

        public HealthMetricData Build() => new()
        {
            NormalizedTimestamp = _timestamp,
            Value = _value,
            Unit = _unit,
            Provider = _provider
        };

        public List<HealthMetricData> BuildSeries(int count, TimeSpan interval)
        {
            var series = new List<HealthMetricData>();
            var current = _timestamp;
            for (int i = 0; i < count; i++)
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
        private string _metricId = "metric.test";
        private DateTimeOffset _startTime = DateTimeOffset.UtcNow;
        private TimeSpan _interval = TimeSpan.FromDays(1);
        private decimal? _value = 100m;
        private string _unitSymbol = "kg";
        private int _sampleCount = 10;

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
            for (int i = 0; i < _sampleCount; i++)
            {
                samples.Add(new MetricSample(current, _value));
                current = current.Add(_interval);
            }

            return new MockCanonicalMetricSeries
            {
                MetricId = new CanonicalMetricId(_metricId),
                Samples = samples,
                Unit = new MetricUnit(_unitSymbol, true),
                Time = new TimeSemantics(
                    TimeRepresentation.Point,
                    _startTime,
                    samples.Count > 0 ? samples[^1].Timestamp : null)
            };
        }
    }

    internal class MockCanonicalMetricSeries : ICanonicalMetricSeries
    {
        public CanonicalMetricId MetricId { get; init; } = default!;
        public TimeSemantics Time { get; init; } = default!;
        public IReadOnlyList<MetricSample> Samples { get; init; } = Array.Empty<MetricSample>();
        public MetricUnit Unit { get; init; } = default!;
        public MetricDimension Dimension { get; init; } = MetricDimension.Unknown;
        public MetricProvenance Provenance { get; init; } = new("Test", "Test", "1.0");
        public MetricQuality Quality { get; init; } = new(DataCompleteness.Complete, ValidationStatus.Validated);
    }
}
