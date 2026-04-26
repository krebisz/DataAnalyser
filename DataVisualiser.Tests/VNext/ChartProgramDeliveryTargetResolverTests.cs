using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class ChartProgramDeliveryTargetResolverTests
{
    [Theory]
    [InlineData(ChartProgramKind.Main, "MainChart")]
    [InlineData(ChartProgramKind.Normalized, "NormalizedChart")]
    [InlineData(ChartProgramKind.Difference, "DiffRatioChart")]
    [InlineData(ChartProgramKind.Ratio, "DiffRatioChart")]
    [InlineData(ChartProgramKind.Transform, "TransformChart")]
    [InlineData(ChartProgramKind.Distribution, "DistributionChart")]
    [InlineData(ChartProgramKind.WeekdayTrend, "WeekdayTrendChart")]
    [InlineData(ChartProgramKind.BarPie, "BarPieChart")]
    [InlineData(ChartProgramKind.SyncfusionSunburst, "SyncfusionSunburst")]
    public void ResolveDefaultTarget_ShouldReturnCanonicalTarget(ChartProgramKind kind, string expectedTarget)
    {
        Assert.Equal(expectedTarget, ChartProgramDeliveryTargetResolver.ResolveDefaultTarget(kind));
    }

    [Fact]
    public void CreateDelivery_ShouldCreateHierarchyContractForSyncfusion()
    {
        var delivery = ChartProgramDeliveryTargetResolver.CreateDelivery(ChartProgramKind.SyncfusionSunburst);

        Assert.Equal(ConsumerKind.HierarchyChart, delivery.ConsumerKind);
        Assert.Equal("SyncfusionSunburst", delivery.DeliveryTarget);
    }

    [Fact]
    public void CreateDelivery_ShouldUseOverrideTargetWhenProvided()
    {
        var delivery = ChartProgramDeliveryTargetResolver.CreateDelivery(ChartProgramKind.Main, "CustomSurface");

        Assert.Equal(ConsumerKind.Chart, delivery.ConsumerKind);
        Assert.Equal("CustomSurface", delivery.DeliveryTarget);
    }
}
