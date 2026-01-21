using DataVisualiser.Core.Validation.Parity;

namespace DataVisualiser.Tests.Parity;

public sealed class ChartComputationParityHarnessTests
{
    [Fact]
    public void Validate_ShouldPass_WhenSeriesMatch()
    {
        var harness = new ChartComputationParityHarness();
        var context = new StrategyParityContext { StrategyName = "Test", MetricIdentity = "Metric" };

        var legacy = new LegacyExecutionResult
        {
            Series = new List<ParitySeries>
            {
                new()
                {
                    SeriesKey = "Primary",
                    Points = new List<ParityPoint>
                    {
                        new() { Time = new DateTime(2024, 01, 01), Value = 1.0 },
                        new() { Time = new DateTime(2024, 01, 02), Value = 2.0 }
                    }
                }
            }
        };

        var cms = new CmsExecutionResult
        {
            Series = legacy.Series.Select(s => new ParitySeries
            {
                SeriesKey = s.SeriesKey,
                Points = s.Points.Select(p => new ParityPoint { Time = p.Time, Value = p.Value }).ToList()
            }).ToList()
        };

        var result = harness.Validate(context, () => legacy, () => cms);

        Assert.True(result.Passed);
    }

    [Fact]
    public void Validate_ShouldFail_OnTimestampMismatch()
    {
        var harness = new ChartComputationParityHarness();
        var context = new StrategyParityContext { StrategyName = "Test", MetricIdentity = "Metric" };

        var legacy = new LegacyExecutionResult
        {
            Series = new List<ParitySeries>
            {
                new()
                {
                    SeriesKey = "Primary",
                    Points = new List<ParityPoint>
                    {
                        new() { Time = new DateTime(2024, 01, 01), Value = 1.0 }
                    }
                }
            }
        };

        var cms = new CmsExecutionResult
        {
            Series = new List<ParitySeries>
            {
                new()
                {
                    SeriesKey = "Primary",
                    Points = new List<ParityPoint>
                    {
                        new() { Time = new DateTime(2024, 01, 02), Value = 1.0 }
                    }
                }
            }
        };

        var result = harness.Validate(context, () => legacy, () => cms);

        Assert.False(result.Passed);
        Assert.Contains(result.Failures, f => f.Layer == ParityLayer.TemporalParity);
    }
}
