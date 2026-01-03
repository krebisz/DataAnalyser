using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Strategies;

public sealed class MultiMetricStrategyTests
{
    private static readonly DateTime From = new(2024, 01, 01);
    private static readonly DateTime To   = new(2024, 01, 10);

    [Fact]
    public void Compute_ShouldReturnNull_WhenAllSeriesEmpty()
    {
        var series = new List<IEnumerable<MetricData>>
        {
                Enumerable.Empty<MetricData>(),
                Enumerable.Empty<MetricData>()
        };

        var strategy = new MultiMetricStrategy(series, new[]
        {
                "A",
                "B"
        }, From, To);

        var result = strategy.Compute();

        Assert.Null(result);
    }

    [Fact]
    public void Compute_ShouldSkipEmptySeries_AndReturnRemaining()
    {
        var series = new List<IEnumerable<MetricData>>
        {
                Enumerable.Empty<MetricData>(),
                TestDataBuilders.HealthMetricData().
                                 WithTimestamp(From).
                                 WithUnit("kg").
                                 BuildSeries(5, TimeSpan.FromDays(1))
        };

        var strategy = new MultiMetricStrategy(series, new[]
        {
                "Empty",
                "Valid"
        }, From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Single(result!.Series!);
        Assert.Equal("Valid", result.Series![0].DisplayName);
    }

    [Fact]
    public void Compute_ShouldEmitOneSeriesResult_PerInputSeries()
    {
        var series = new List<IEnumerable<MetricData>>
        {
                TestDataBuilders.HealthMetricData().
                                 WithTimestamp(From).
                                 WithUnit("kg").
                                 BuildSeries(5, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().
                                 WithTimestamp(From).
                                 WithUnit("kg").
                                 BuildSeries(5, TimeSpan.FromDays(1))
        };

        var strategy = new MultiMetricStrategy(series, new[]
        {
                "A",
                "B"
        }, From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal(2, result!.Series!.Count);
    }

    [Fact]
    public void Compute_ShouldOrderTimestamps_PerSeries()
    {
        var series = new List<IEnumerable<MetricData>>
        {
                new List<MetricData>
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
                }
        };

        var strategy = new MultiMetricStrategy(series, new[]
        {
                "A"
        }, From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        var timestamps = result!.Series![0].Timestamps;
        Assert.True(timestamps.SequenceEqual(timestamps.OrderBy(t => t)));
    }

    [Fact]
    public void Compute_ShouldGenerateSmoothedValues_PerSeries()
    {
        var series = new List<IEnumerable<MetricData>>
        {
                TestDataBuilders.HealthMetricData().
                                 WithTimestamp(From).
                                 WithUnit("kg").
                                 BuildSeries(10, TimeSpan.FromDays(1))
        };

        var strategy = new MultiMetricStrategy(series, new[]
        {
                "A"
        }, From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        var sr = result!.Series![0];
        Assert.NotNull(sr.Smoothed);
        Assert.Equal(sr.RawValues.Count, sr.Smoothed!.Count);
    }

    [Fact]
    public void Compute_ShouldSetUnit_FromFirstNonEmptySeries()
    {
        var series = new List<IEnumerable<MetricData>>
        {
                TestDataBuilders.HealthMetricData().
                                 WithTimestamp(From).
                                 WithUnit("kg").
                                 BuildSeries(5, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().
                                 WithTimestamp(From).
                                 WithUnit("lb").
                                 BuildSeries(5, TimeSpan.FromDays(1))
        };

        var strategy = new MultiMetricStrategy(series, new[]
        {
                "A",
                "B"
        }, From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal("kg", result!.Unit);
    }

    [Fact]
    public void Compute_ShouldHandleCanonicalMetricSeries_Input()
    {
        var cmsSeries = new[]
        {
                TestDataBuilders.CanonicalMetricSeries().
                                 WithMetricId("metric.test.multi").
                                 WithUnit("kg").
                                 WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).
                                 WithSampleCount(5).
                                 Build(),
                TestDataBuilders.CanonicalMetricSeries().
                                 WithMetricId("metric.test.multi").
                                 WithUnit("kg").
                                 WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).
                                 WithSampleCount(5).
                                 Build()
        };

        var strategy = new MultiMetricStrategy(cmsSeries, new[]
        {
                "A",
                "B"
        }, From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal(2, result!.Series!.Count);
        Assert.Equal("kg", result.Unit);
    }
}