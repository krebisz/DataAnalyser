using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Tests.Strategies;

public sealed class NormalizedStrategyTests
{
    private static readonly DateTime From = new(2024, 01, 01);
    private static readonly DateTime To   = new(2024, 01, 10);

    [Fact]
    public void Compute_ShouldReturnNull_WhenBothSeriesEmpty()
    {
        var left = Enumerable.Empty<HealthMetricData>();
        var right = Enumerable.Empty<HealthMetricData>();

        var strategy = new NormalizedStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.Null(result);
    }

    [Fact]
    public void Compute_ShouldReturnResult_WithNaNs_WhenNoOverlappingTimestamps()
    {
        var left = new List<HealthMetricData>
        {
                new()
                {
                        NormalizedTimestamp = From,
                        Value = 10m,
                        Unit = "kg"
                }
        };

        var right = new List<HealthMetricData>
        {
                new()
                {
                        NormalizedTimestamp = To.AddDays(5),
                        Value = 20m,
                        Unit = "kg"
                }
        };

        var strategy = new NormalizedStrategy(left, right, "L", "R", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.True(result!.PrimaryRawValues.All(double.IsNaN));
        Assert.True(result.PrimarySmoothed.All(double.IsNaN));
    }

    [Fact]
    public void Compute_ShouldNormalizePrimaryRawValues_ForPercentageOfMax_EvenWhenAlignmentHasGaps()
    {
        var left = new List<HealthMetricData>
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

        var right = new List<HealthMetricData>
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
        var left = new List<HealthMetricData>
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

        var right = new List<HealthMetricData>
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

        // Baseline series is a straight 100% line (per MathHelper.RelativeToMax overload)
        Assert.All(result.SecondaryRawValues!, v => Assert.Equal(100.0, v));
        Assert.All(result.SecondarySmoothed!, v => Assert.Equal(100.0, v));
    }

    [Fact]
    public void Compute_ShouldGenerateSmoothedSeries()
    {
        var left = new List<HealthMetricData>
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

        var right = new List<HealthMetricData>
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
        var left = new List<HealthMetricData>
        {
                new()
                {
                        NormalizedTimestamp = From,
                        Value = 10m,
                        Unit = "kg"
                }
        };

        var right = new List<HealthMetricData>
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
    public void SecondaryLabel_ShouldBeSet_ForRelativeToMax()
    {
        var strategy = new NormalizedStrategy(Enumerable.Empty<HealthMetricData>(), Enumerable.Empty<HealthMetricData>(), "L", "R", From, To, NormalizationMode.RelativeToMax);

        Assert.Equal("R (baseline)", strategy.SecondaryLabel);
    }
}