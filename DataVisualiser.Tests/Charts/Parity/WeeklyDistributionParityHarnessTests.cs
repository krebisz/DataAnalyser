using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Validation.Parity;

namespace DataVisualiser.Tests.Charts.Parity;

public class WeeklyDistributionParityHarnessTests
{
    [Fact]
    public void WeeklyDistribution_Legacy_vs_Cms_Parity_Passes_On_ExtendedResult()
    {
        // Arrange
        var start = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero);

        var cms = TestDataBuilders.CanonicalMetricSeries().
                                   WithMetricId("metric.body_weight").
                                   WithStartTime(start).
                                   WithInterval(TimeSpan.FromDays(1)).
                                   WithValue(100m).
                                   WithUnit("kg").
                                   WithSampleCount(10).
                                   Build();

        // Build equivalent legacy MetricData samples
        var legacy = new List<MetricData>();
        var current = start;
        for (var i = 0; i < 10; i++)
        {
            legacy.Add(TestDataBuilders.HealthMetricData().
                                        WithTimestamp(current.DateTime).
                                        WithValue(100m).
                                        WithUnit("kg").
                                        Build());

            current = current.AddDays(1);
        }

        var from = start.DateTime;
        var to = start.AddDays(9).
                       DateTime;

        var legacyStrategy = new WeeklyDistributionStrategy(legacy, "weight", from, to);
        var cmsStrategy = new CmsWeeklyDistributionStrategy(cms, from, to, "weight");

        legacyStrategy.Compute();
        cmsStrategy.Compute();

        var harness = new WeeklyDistributionParityHarness();

        // Act
        var parity = harness.Validate(new StrategyParityContext
        {
                StrategyName = "WeeklyDistribution",
                MetricIdentity = "weight",
                Mode = ParityMode.Strict,
                Tolerance = new ParityTolerance
                {
                        AllowFloatingPointDrift = true,
                        ValueEpsilon = 1e-9
                }
        }, () => WeeklyDistributionParityHarness.ToLegacyExecutionResult(legacyStrategy.ExtendedResult), () => WeeklyDistributionParityHarness.ToCmsExecutionResult(cmsStrategy.ExtendedResult));

        // Assert
        Assert.True(parity.Passed);
    }
}