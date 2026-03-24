using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Parity;

public sealed class DifferenceParityTests
{
    [Fact]
    public void Parity_ShouldPass_ForCmsAndLegacy()
    {
        var from = DateTime.Now.Date.AddDays(-10);
        var to = from.AddDays(9).AddHours(23).AddMinutes(59).AddSeconds(59);
        var interval = TimeSpan.FromDays(1);

        var legacyLeft = TestDataBuilders.HealthMetricData().WithTimestamp(from).WithValue(100m).WithUnit("kg").BuildSeries(10, interval);
        var legacyRight = TestDataBuilders.HealthMetricData().WithTimestamp(from).WithValue(30m).WithUnit("kg").BuildSeries(10, interval);
        var cmsLeft = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.left").WithStartTime(new DateTimeOffset(from)).WithInterval(interval).WithValue(100m).WithUnit("kg").WithSampleCount(10).Build();
        var cmsRight = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.right").WithStartTime(new DateTimeOffset(from)).WithInterval(interval).WithValue(30m).WithUnit("kg").WithSampleCount(10).Build();

        var legacyResult = new DifferenceStrategy(legacyLeft, legacyRight, "Left", "Right", from, to).Compute();
        var cmsResult = new DifferenceStrategy(cmsLeft, cmsRight, "Left", "Right", from, to).Compute();

        AssertEquivalent(legacyResult, cmsResult);
    }

    private static void AssertEquivalent(ChartComputationResult? legacyResult, ChartComputationResult? cmsResult)
    {
        Assert.NotNull(legacyResult);
        Assert.NotNull(cmsResult);
        Assert.Equal(legacyResult!.Timestamps.Count, cmsResult!.Timestamps.Count);
        Assert.Equal(legacyResult.PrimaryRawValues.Count, cmsResult.PrimaryRawValues.Count);
        Assert.Equal(legacyResult.Unit, cmsResult.Unit);

        for (var i = 0; i < legacyResult.PrimaryRawValues.Count; i++)
        {
            Assert.True(Math.Abs(legacyResult.PrimaryRawValues[i] - cmsResult.PrimaryRawValues[i]) < 0.0001, $"Raw mismatch at {i}");
            Assert.True(Math.Abs(legacyResult.PrimarySmoothed[i] - cmsResult.PrimarySmoothed[i]) < 0.0001, $"Smoothed mismatch at {i}");
        }
    }
}
