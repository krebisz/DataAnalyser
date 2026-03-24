using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Core.Validation.Parity;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Charts.Parity;

public sealed class HourlyDistributionParityHarnessTests
{
    [Fact]
    public void HourlyDistribution_Legacy_vs_Cms_Parity_Passes_On_ExtendedResult()
    {
        var localOffset = TimeZoneInfo.Local.GetUtcOffset(new DateTime(2024, 01, 01, 12, 0, 0));
        var start = new DateTimeOffset(2024, 01, 01, 0, 0, 0, localOffset);

        var cms = TestDataBuilders.CanonicalMetricSeries()
            .WithMetricId("metric.body_weight.hourly")
            .WithStartTime(start)
            .WithInterval(TimeSpan.FromHours(1))
            .WithValue(100m)
            .WithUnit("kg")
            .WithSampleCount(24)
            .Build();

        var legacy = new List<MetricData>();
        var current = start;
        for (var i = 0; i < 24; i++)
        {
            legacy.Add(TestDataBuilders.HealthMetricData().WithTimestamp(current.DateTime).WithValue(100m).WithUnit("kg").Build());
            current = current.AddHours(1);
        }

        var from = start.DateTime;
        var to = start.AddHours(23).DateTime;

        var legacyStrategy = new HourlyDistributionStrategy(legacy, "weight", from, to);
        var cmsStrategy = new CmsHourlyDistributionStrategy(cms, from, to, "weight");

        legacyStrategy.Compute();
        cmsStrategy.Compute();

        var harness = new HourlyDistributionParityHarness();

        var parity = harness.Validate(new StrategyParityContext
            {
                StrategyName = "HourlyDistribution",
                MetricIdentity = "weight",
                Mode = ParityMode.Strict,
                Tolerance = new ParityTolerance
                {
                    AllowFloatingPointDrift = true,
                    ValueEpsilon = 1e-9
                }
            },
            () => harness.ToLegacyExecutionResult(legacyStrategy.ExtendedResult),
            () => harness.ToCmsExecutionResult(cmsStrategy.ExtendedResult));

        Assert.True(parity.Passed);
    }
}
