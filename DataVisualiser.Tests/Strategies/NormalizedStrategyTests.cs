using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;

namespace DataVisualiser.Tests.Strategies;

public sealed class NormalizedStrategyTests
{
    private static readonly DateTime From = new(2024, 01, 01);
    private static readonly DateTime To = new(2024, 01, 10);

    [Fact]
    public void Compute_ShouldReturnNull_WhenBothSeriesEmpty()
    {
        var left = Enumerable.Empty<MetricData>();
        var right = Enumerable.Empty<MetricData>();

        var strategy = new NormalizedStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.Null(result);
    }

    [Fact]
    public void Compute_ShouldNormalizeSparseSeries_WhenTimestampsDoNotOverlap()
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
                NormalizedTimestamp = From.AddDays(2),
                Value = 20m,
                Unit = "kg"
            }
        };

        var right = new List<MetricData>
        {
            new()
            {
                NormalizedTimestamp = From.AddDays(1),
                Value = 100m,
                Unit = "kg"
            },
            new()
            {
                NormalizedTimestamp = From.AddDays(3),
                Value = 200m,
                Unit = "kg"
            }
        };

        var strategy = new NormalizedStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal([50.0, double.NaN, 100.0, double.NaN], result!.PrimaryRawValues);
        Assert.Equal([double.NaN, 50.0, double.NaN, 100.0], result.SecondaryRawValues!);
    }

    [Theory]
    [InlineData(NormalizationMode.ZeroToOne)]
    [InlineData(NormalizationMode.PercentageOfMax)]
    [InlineData(NormalizationMode.RelativeToMax)]
    public void Compute_ShouldRenderAvailableValuesForEveryMode_WhenSelectedSeriesDoNotOverlap(NormalizationMode mode)
    {
        var left = new List<MetricData>
        {
            new() { NormalizedTimestamp = From, Value = 10m, Unit = "kg" },
            new() { NormalizedTimestamp = From.AddDays(2), Value = 20m, Unit = "kg" }
        };
        var right = new List<MetricData>
        {
            new() { NormalizedTimestamp = From.AddDays(1), Value = 100m, Unit = "kg" },
            new() { NormalizedTimestamp = From.AddDays(3), Value = 200m, Unit = "kg" }
        };

        var strategy = new NormalizedStrategy(left, right, "L", "R", From, To, mode);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Contains(result!.PrimaryRawValues, value => !double.IsNaN(value));
        Assert.Contains(result.SecondaryRawValues!, value => !double.IsNaN(value));
    }

    [Fact]
    public void Compute_ShouldNormalizePrimaryRawValues_ForPercentageOfMax_EvenWhenAlignmentHasGaps()
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
            },
            new()
            {
                NormalizedTimestamp = From.AddDays(2),
                Value = 30m,
                Unit = "kg"
            },
            new()
            {
                NormalizedTimestamp = From.AddDays(3),
                Value = 40m,
                Unit = "kg"
            },
            new()
            {
                NormalizedTimestamp = From.AddDays(4),
                Value = 50m,
                Unit = "kg"
            }
        };

        var right = new List<MetricData>
        {
            new()
            {
                NormalizedTimestamp = From,
                Value = 5m,
                Unit = "kg"
            }
        };

        var strategy = new NormalizedStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);

        Assert.All(result!.PrimaryRawValues, v => Assert.InRange(v, 0.0, 100.0));
        Assert.Contains(100.0, result.PrimaryRawValues);
        Assert.DoesNotContain(result.PrimaryRawValues, double.IsNaN);
    }

    [Fact]
    public void Compute_ShouldReturnTwoSeries_ForRelativeToMax()
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
                Value = 5m,
                Unit = "kg"
            },
            new()
            {
                NormalizedTimestamp = From.AddDays(1),
                Value = 10m,
                Unit = "kg"
            }
        };

        var strategy = new NormalizedStrategy(left, right, "L", "R", From, To, NormalizationMode.RelativeToMax);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.NotNull(result!.SecondaryRawValues);
        Assert.NotNull(result.SecondarySmoothed);
        Assert.Equal([50.0, 100.0], result.PrimaryRawValues);
        Assert.Equal([25.0, 50.0], result.SecondaryRawValues!);
        Assert.All(result.SecondarySmoothed!, v => Assert.InRange(v, 0.0, 100.0));
    }

    [Fact]
    public void Compute_ShouldGenerateSmoothedSeries()
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
            },
            new()
            {
                NormalizedTimestamp = From.AddDays(2),
                Value = 30m,
                Unit = "kg"
            },
            new()
            {
                NormalizedTimestamp = From.AddDays(3),
                Value = 40m,
                Unit = "kg"
            }
        };

        var right = new List<MetricData>
        {
            new()
            {
                NormalizedTimestamp = From,
                Value = 5m,
                Unit = "kg"
            },
            new()
            {
                NormalizedTimestamp = From.AddDays(1),
                Value = 10m,
                Unit = "kg"
            },
            new()
            {
                NormalizedTimestamp = From.AddDays(2),
                Value = 15m,
                Unit = "kg"
            },
            new()
            {
                NormalizedTimestamp = From.AddDays(3),
                Value = 20m,
                Unit = "kg"
            }
        };

        var strategy = new NormalizedStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal(result!.PrimaryRawValues.Count, result.PrimarySmoothed.Count);
    }

    [Fact]
    public void Compute_ShouldResolveUnit_FromInputSeries()
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
                Value = 5m,
                Unit = "kg"
            }
        };

        var strategy = new NormalizedStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal("kg", result!.Unit);
    }

    [Fact]
    public void Labels_ShouldExposeIndependentSeriesNames()
    {
        var strategy = new NormalizedStrategy(Enumerable.Empty<MetricData>(), Enumerable.Empty<MetricData>(), "L", "R", From, To, NormalizationMode.RelativeToMax);

        Assert.Equal("L", strategy.PrimaryLabel);
        Assert.Equal("R", strategy.SecondaryLabel);
    }

    [Fact]
    public void Compute_ShouldSupportCmsInputs()
    {
        var left = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.left").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(10m).WithUnit("kg").WithSampleCount(5).Build();
        var right = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.right").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(20m).WithUnit("kg").WithSampleCount(5).Build();

        var strategy = new NormalizedStrategy(left, right, "L", "R", From, To, NormalizationMode.PercentageOfMax);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal(5, result!.PrimaryRawValues.Count);
    }
}
