using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Parity;

public sealed class NormalizedParityTests
{
    [Fact]
    public void Parity_ShouldPass_ForPercentageOfMax()
    {
        var from = DateTime.Now.Date.AddDays(-10);
        var to = from.AddDays(9).AddHours(23).AddMinutes(59).AddSeconds(59);
        var interval = TimeSpan.FromDays(1);

        var legacyLeft = TestDataBuilders.HealthMetricData().WithTimestamp(from).WithValue(100m).WithUnit("kg").BuildSeries(10, interval);
        var legacyRight = TestDataBuilders.HealthMetricData().WithTimestamp(from).WithValue(50m).WithUnit("kg").BuildSeries(10, interval);
        var cmsLeft = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.left").WithStartTime(new DateTimeOffset(from)).WithInterval(interval).WithValue(100m).WithUnit("kg").WithSampleCount(10).Build();
        var cmsRight = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.right").WithStartTime(new DateTimeOffset(from)).WithInterval(interval).WithValue(50m).WithUnit("kg").WithSampleCount(10).Build();

        var legacyResult = new NormalizedStrategy(legacyLeft, legacyRight, "Left", "Right", from, to, NormalizationMode.PercentageOfMax).Compute();
        var cmsResult = new NormalizedStrategy(cmsLeft, cmsRight, "Left", "Right", from, to, NormalizationMode.PercentageOfMax).Compute();

        AssertEquivalent(legacyResult, cmsResult);
    }

    [Fact]
    public void Parity_ShouldPass_ForRelativeToMax()
    {
        var from = DateTime.Now.Date.AddDays(-10);
        var to = from.AddDays(9).AddHours(23).AddMinutes(59).AddSeconds(59);
        var interval = TimeSpan.FromDays(1);

        var legacyLeft = TestDataBuilders.HealthMetricData().WithTimestamp(from).WithValue(100m).WithUnit("kg").BuildSeries(10, interval);
        var legacyRight = TestDataBuilders.HealthMetricData().WithTimestamp(from).WithValue(25m).WithUnit("kg").BuildSeries(10, interval);
        var cmsLeft = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.left").WithStartTime(new DateTimeOffset(from)).WithInterval(interval).WithValue(100m).WithUnit("kg").WithSampleCount(10).Build();
        var cmsRight = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.right").WithStartTime(new DateTimeOffset(from)).WithInterval(interval).WithValue(25m).WithUnit("kg").WithSampleCount(10).Build();

        var legacyResult = new NormalizedStrategy(legacyLeft, legacyRight, "Left", "Right", from, to, NormalizationMode.RelativeToMax).Compute();
        var cmsResult = new NormalizedStrategy(cmsLeft, cmsRight, "Left", "Right", from, to, NormalizationMode.RelativeToMax).Compute();

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
            AssertEqualDouble(legacyResult.PrimaryRawValues[i], cmsResult.PrimaryRawValues[i]);
            AssertEqualDouble(legacyResult.PrimarySmoothed[i], cmsResult.PrimarySmoothed[i]);
        }

        Assert.Equal(legacyResult.SecondaryRawValues?.Count ?? 0, cmsResult.SecondaryRawValues?.Count ?? 0);
        if (legacyResult.SecondaryRawValues != null && cmsResult.SecondaryRawValues != null)
        {
            for (var i = 0; i < legacyResult.SecondaryRawValues.Count; i++)
            {
                AssertEqualDouble(legacyResult.SecondaryRawValues[i], cmsResult.SecondaryRawValues[i]);
                AssertEqualDouble(legacyResult.SecondarySmoothed![i], cmsResult.SecondarySmoothed![i]);
            }
        }
    }

    private static void AssertEqualDouble(double expected, double actual)
    {
        if (double.IsNaN(expected) && double.IsNaN(actual))
            return;

        Assert.True(Math.Abs(expected - actual) < 0.0001, $"Value mismatch: expected={expected}, actual={actual}");
    }
}
