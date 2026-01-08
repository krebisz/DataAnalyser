using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Strategies;

public sealed class RatioStrategyTests
{
    private static readonly DateTime From = new(2024, 01, 01);
    private static readonly DateTime To = new(2024, 01, 10);

    [Fact]
    public void Compute_ShouldReturnNull_WhenBothSeriesEmpty()
    {
        var left = Enumerable.Empty<MetricData>();
        var right = Enumerable.Empty<MetricData>();

        var strategy = new RatioStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.Null(result);
    }

    [Fact]
    public void Compute_ShouldReturnNull_WhenOneSeriesEmpty()
    {
        var left = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithUnit("kg").BuildSeries(5, TimeSpan.FromDays(1));

        var right = Enumerable.Empty<MetricData>();

        var strategy = new RatioStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.Null(result);
    }

    [Fact]
    public void Compute_ShouldAlignByIndex_UsingShortestSeries()
    {
        var left = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithUnit("kg").BuildSeries(8, TimeSpan.FromDays(1));

        var right = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithUnit("kg").BuildSeries(5, TimeSpan.FromDays(1));

        var strategy = new RatioStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal(5, result!.PrimaryRawValues.Count);
        Assert.Equal(5, result.Timestamps.Count);
    }

    [Fact]
    public void Compute_ShouldCalculateLeftDividedByRight()
    {
        var left = new List<MetricData>
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
                        Value = 20m,
                        Unit = "kg"
                }
        };

        var right = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From,
                        Value = 2m,
                        Unit = "kg"
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(1),
                        Value = 4m,
                        Unit = "kg"
                }
        };

        var strategy = new RatioStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal(new[]
                {
                        5.0,
                        5.0
                },
                result!.PrimaryRawValues);
    }

    [Fact]
    public void Compute_ShouldProduceNaN_WhenRightValueIsZero()
    {
        var left = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From,
                        Value = 10m,
                        Unit = "kg"
                }
        };

        var right = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From,
                        Value = 0m,
                        Unit = "kg"
                }
        };

        var strategy = new RatioStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.True(double.IsNaN(result!.PrimaryRawValues[0]));
    }

    [Fact]
    public void Compute_ShouldGenerateSmoothedValues()
    {
        var left = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithUnit("kg").BuildSeries(10, TimeSpan.FromDays(1));

        var right = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithUnit("kg").BuildSeries(10, TimeSpan.FromDays(1));

        var strategy = new RatioStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.NotNull(result!.PrimarySmoothed);
        Assert.Equal(result.PrimaryRawValues.Count, result.PrimarySmoothed.Count);
    }

    [Fact]
    public void Compute_ShouldResolveUnit_AsRatio_WhenBothUnitsPresent()
    {
        var left = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithUnit("kg").BuildSeries(5, TimeSpan.FromDays(1));

        var right = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithUnit("m").BuildSeries(5, TimeSpan.FromDays(1));

        var strategy = new RatioStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal("kg/m", result!.Unit);
    }

    [Fact]
    public void Compute_ShouldReturnNullUnit_WhenEitherUnitMissing()
    {
        var left = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithUnit(null).BuildSeries(5, TimeSpan.FromDays(1));

        var right = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithUnit("kg").BuildSeries(5, TimeSpan.FromDays(1));

        var strategy = new RatioStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Null(result!.Unit);
    }
}