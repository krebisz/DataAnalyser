using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Strategies;

public sealed class SingleMetricStrategyTests
{
    private static readonly DateTime From = new(2024, 01, 01);
    private static readonly DateTime To = From.AddDays(9).AddHours(23).AddMinutes(59).AddSeconds(59);

    [Fact]
    public void Compute_ShouldReturnNull_WhenDataIsEmpty()
    {
        var data = Enumerable.Empty<MetricData>();

        var strategy = new SingleMetricStrategy(data, "Test", From, To);

        var result = strategy.Compute();

        Assert.Null(result);
    }

    [Fact]
    public void Compute_ShouldReturnNull_WhenAllValuesAreNull()
    {
        var data = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From,
                        Value = null,
                        Unit = "kg"
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(1),
                        Value = null,
                        Unit = "kg"
                }
        };

        var strategy = new SingleMetricStrategy(data, "Test", From, To);

        var result = strategy.Compute();

        Assert.Null(result);
    }

    [Fact]
    public void Compute_ShouldFilterNullValues()
    {
        var data = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From,
                        Value = 10m,
                        Unit = "kg"
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(1),
                        Value = null,
                        Unit = "kg"
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(2),
                        Value = 20m,
                        Unit = "kg"
                }
        };

        var strategy = new SingleMetricStrategy(data, "Test", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal(2, result!.PrimaryRawValues.Count);
        Assert.DoesNotContain(double.NaN, result.PrimaryRawValues);
    }

    [Fact]
    public void Compute_ShouldOrderByTimestamp()
    {
        var data = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From.AddDays(2),
                        Value = 30m,
                        Unit = "kg"
                },
                new()
                {
                        NormalizedTimestamp = From,
                        Value = 10m,
                        Unit = "kg"
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(1),
                        Value = 20m,
                        Unit = "kg"
                }
        };

        var strategy = new SingleMetricStrategy(data, "Test", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.True(result!.Timestamps.SequenceEqual(result.Timestamps.OrderBy(t => t)));
    }

    [Fact]
    public void Compute_ShouldGenerateSmoothedData()
    {
        var data = TestDataBuilders.HealthMetricData().WithUnit("kg").BuildSeries(10, TimeSpan.FromDays(1));

        var strategy = new SingleMetricStrategy(data, "Test", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.NotNull(result!.PrimarySmoothed);
        Assert.Equal(result.PrimaryRawValues.Count, result.PrimarySmoothed.Count);
    }

    [Fact]
    public void Compute_ShouldSetUnit_FromFirstDataPoint()
    {
        var data = TestDataBuilders.HealthMetricData().WithUnit("kg").BuildSeries(5, TimeSpan.FromDays(1));

        var strategy = new SingleMetricStrategy(data, "Test", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal("kg", result!.Unit);
    }

    [Fact]
    public void Compute_ShouldHandleCmsData_WhenCmsConstructorUsed()
    {
        var cms = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.test.single").WithUnit("kg").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithSampleCount(10).Build();

        var strategy = new SingleMetricStrategy(cms, "Test", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal(10, result!.PrimaryRawValues.Count);
        Assert.Equal("kg", result.Unit);
    }
}
