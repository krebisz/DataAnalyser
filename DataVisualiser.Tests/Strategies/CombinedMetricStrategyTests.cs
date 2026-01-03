using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Strategies;

public sealed class CombinedMetricStrategyTests
{
    private static readonly DateTime From = new(2024, 01, 01);
    private static readonly DateTime To   = new(2024, 01, 10);

    [Fact]
    public void Compute_ShouldReturnNull_WhenBothSeriesEmpty()
    {
        var left = Enumerable.Empty<HealthMetricData>();
        var right = Enumerable.Empty<HealthMetricData>();

        var strategy = new CombinedMetricStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.Null(result);
    }

    [Fact]
    public void Compute_ShouldReturnNull_WhenOneSeriesEmpty()
    {
        var left = TestDataBuilders.HealthMetricData().
                                    WithTimestamp(From).
                                    WithUnit("kg").
                                    BuildSeries(5, TimeSpan.FromDays(1));

        var right = Enumerable.Empty<HealthMetricData>();

        var strategy = new CombinedMetricStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.Null(result);
    }

    [Fact]
    public void Compute_ShouldAlignByIndex_UsingShortestSeries()
    {
        var left = TestDataBuilders.HealthMetricData().
                                    WithTimestamp(From).
                                    WithUnit("kg").
                                    BuildSeries(10, TimeSpan.FromDays(1));

        var right = TestDataBuilders.HealthMetricData().
                                     WithTimestamp(From).
                                     WithUnit("kg").
                                     BuildSeries(6, TimeSpan.FromDays(1));

        var strategy = new CombinedMetricStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal(6, result!.PrimaryRawValues.Count);
        Assert.Equal(6, result.SecondaryRawValues!.Count);
        Assert.Equal(6, result.Timestamps.Count);
    }

    [Fact]
    public void Compute_ShouldOrderByTimestamp()
    {
        var left = new List<HealthMetricData>
        {
                new()
                {
                        NormalizedTimestamp = From.AddDays(2),
                        Value = 3m,
                        Unit = "kg"
                },
                new()
                {
                        NormalizedTimestamp = From,
                        Value = 1m,
                        Unit = "kg"
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(1),
                        Value = 2m,
                        Unit = "kg"
                }
        };

        var right = new List<HealthMetricData>
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

        var strategy = new CombinedMetricStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.True(result!.Timestamps.SequenceEqual(result.Timestamps.OrderBy(t => t)));
    }

    [Fact]
    public void Compute_ShouldGenerateSmoothedSeries_ForBothMetrics()
    {
        var left = TestDataBuilders.HealthMetricData().
                                    WithTimestamp(From).
                                    WithUnit("kg").
                                    BuildSeries(10, TimeSpan.FromDays(1));

        var right = TestDataBuilders.HealthMetricData().
                                     WithTimestamp(From).
                                     WithUnit("kg").
                                     BuildSeries(10, TimeSpan.FromDays(1));

        var strategy = new CombinedMetricStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.NotNull(result!.PrimarySmoothed);
        Assert.NotNull(result.SecondarySmoothed);
        Assert.Equal(result.PrimaryRawValues.Count, result.PrimarySmoothed.Count);
        Assert.Equal(result.SecondaryRawValues!.Count, result.SecondarySmoothed!.Count);
    }

    [Fact]
    public void Compute_ShouldResolveUnit_WhenUnitsMatch()
    {
        var left = TestDataBuilders.HealthMetricData().
                                    WithTimestamp(From).
                                    WithUnit("kg").
                                    BuildSeries(5, TimeSpan.FromDays(1));

        var right = TestDataBuilders.HealthMetricData().
                                     WithTimestamp(From).
                                     WithUnit("kg").
                                     BuildSeries(5, TimeSpan.FromDays(1));

        var strategy = new CombinedMetricStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal("kg", result!.Unit);
    }

    [Fact]
    public void Compute_ShouldPreferLeftUnit_WhenUnitsDiffer()
    {
        var left = TestDataBuilders.HealthMetricData().
                                    WithTimestamp(From).
                                    WithUnit("kg").
                                    BuildSeries(5, TimeSpan.FromDays(1));

        var right = TestDataBuilders.HealthMetricData().
                                     WithTimestamp(From).
                                     WithUnit("lb").
                                     BuildSeries(5, TimeSpan.FromDays(1));

        var strategy = new CombinedMetricStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal("kg", result!.Unit);
    }
}