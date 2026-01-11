using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Core.Validation.Parity;

namespace DataVisualiser.Tests.Parity;

/// <summary>
///     Parity validation tests for CombinedMetricStrategy (legacy) vs CombinedMetricStrategy (CMS).
///     Validates that legacy and CMS paths produce equivalent results.
/// </summary>
public class CombinedMetricParityTests
{
    private readonly CombinedMetricParityHarness _harness;

    public CombinedMetricParityTests()
    {
        _harness = new CombinedMetricParityHarness();
    }

    [Fact]
    public void Parity_ShouldPass_WithIdenticalData()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(-10);
        var to = DateTime.UtcNow;
        var interval = TimeSpan.FromDays(1);

        // Create matching legacy data
        var leftLegacy = TestDataBuilders.HealthMetricData().WithTimestamp(from).WithValue(100m).WithUnit("kg").BuildSeries(10, interval);

        var rightLegacy = TestDataBuilders.HealthMetricData().WithTimestamp(from).WithValue(50m).WithUnit("kg").BuildSeries(10, interval);

        // Create matching CMS data
        var leftCms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(new DateTimeOffset(from)).WithInterval(interval).WithValue(100m).WithUnit("kg").WithSampleCount(10).Build();

        var rightCms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(new DateTimeOffset(from)).WithInterval(interval).WithValue(50m).WithUnit("kg").WithSampleCount(10).Build();

        var context = new StrategyParityContext
        {
                StrategyName = "CombinedMetric",
                MetricIdentity = "Left|Right",
                Mode = ParityMode.Diagnostic,
                Tolerance = new ParityTolerance
                {
                        ValueEpsilon = 0.0001,
                        AllowFloatingPointDrift = true
                }
        };

        // Act
        var result = _harness.Validate(context,
                () =>
                {
                    var legacyStrategy = new CombinedMetricStrategy(leftLegacy, rightLegacy, "Left", "Right", from, to);
                    return legacyStrategy.Compute()?.ToLegacyExecutionResult() ?? new LegacyExecutionResult
                    {
                            Series = Array.Empty<ParitySeries>()
                    };
                },
                () =>
                {
                    var cmsStrategy = new CombinedMetricStrategy(leftCms, rightCms, "Left", "Right", from, to);
                    return cmsStrategy.Compute()?.ToCmsExecutionResult() ?? new CmsExecutionResult
                    {
                            Series = Array.Empty<ParitySeries>()
                    };
                });

        // Assert
        Assert.True(result.Passed, $"Parity failed: {string.Join("; ", result.Failures.Select(f => $"[{f.Layer}] {f.Message}"))}");
    }

    [Fact]
    public void Parity_ShouldPass_WithEmptyData()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(-10);
        var to = DateTime.UtcNow;

        var context = new StrategyParityContext
        {
                StrategyName = "CombinedMetric",
                MetricIdentity = "Empty|Empty",
                Mode = ParityMode.Diagnostic
        };

        // Act
        var result = _harness.Validate(context,
                () =>
                {
                    var legacyStrategy = new CombinedMetricStrategy(Array.Empty<MetricData>(), Array.Empty<MetricData>(), "Left", "Right", from, to);
                    return legacyStrategy.Compute()?.ToLegacyExecutionResult() ?? new LegacyExecutionResult
                    {
                            Series = Array.Empty<ParitySeries>()
                    };
                },
                () =>
                {
                    var leftCms = TestDataBuilders.CanonicalMetricSeries().WithSampleCount(0).Build();
                    var rightCms = TestDataBuilders.CanonicalMetricSeries().WithSampleCount(0).Build();

                    var cmsStrategy = new CombinedMetricStrategy(leftCms, rightCms, "Left", "Right", from, to);
                    return cmsStrategy.Compute()?.ToCmsExecutionResult() ?? new CmsExecutionResult
                    {
                            Series = Array.Empty<ParitySeries>()
                    };
                });

        // Assert
        Assert.True(result.Passed);
    }

    [Fact]
    public void Parity_ShouldPass_WithMismatchedCounts()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(-10);
        var to = DateTime.UtcNow;
        var interval = TimeSpan.FromDays(1);

        // Left has 10 points, right has 8 points (should align to min)
        var leftLegacy = TestDataBuilders.HealthMetricData().WithTimestamp(from).WithValue(100m).BuildSeries(10, interval);

        var rightLegacy = TestDataBuilders.HealthMetricData().WithTimestamp(from).WithValue(50m).BuildSeries(8, interval);

        var leftCms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(new DateTimeOffset(from)).WithInterval(interval).WithValue(100m).WithSampleCount(10).Build();

        var rightCms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(new DateTimeOffset(from)).WithInterval(interval).WithValue(50m).WithSampleCount(8).Build();

        var context = new StrategyParityContext
        {
                StrategyName = "CombinedMetric",
                MetricIdentity = "Left|Right",
                Mode = ParityMode.Diagnostic,
                Tolerance = new ParityTolerance
                {
                        AllowFloatingPointDrift = true
                }
        };

        // Act
        var result = _harness.Validate(context,
                () =>
                {
                    var legacyStrategy = new CombinedMetricStrategy(leftLegacy, rightLegacy, "Left", "Right", from, to);
                    return legacyStrategy.Compute()?.ToLegacyExecutionResult() ?? new LegacyExecutionResult
                    {
                            Series = Array.Empty<ParitySeries>()
                    };
                },
                () =>
                {
                    var cmsStrategy = new CombinedMetricStrategy(leftCms, rightCms, "Left", "Right", from, to);
                    return cmsStrategy.Compute()?.ToCmsExecutionResult() ?? new CmsExecutionResult
                    {
                            Series = Array.Empty<ParitySeries>()
                    };
                });

        // Assert
        Assert.True(result.Passed);
    }
}
