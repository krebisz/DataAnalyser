using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class AnalyticalIntentFactoryTests
{
    [Theory]
    [InlineData(ChartProgramKind.Main, "MainChart")]
    [InlineData(ChartProgramKind.Normalized, "NormalizedChart")]
    [InlineData(ChartProgramKind.Difference, "DiffRatioChart")]
    [InlineData(ChartProgramKind.Ratio, "DiffRatioChart")]
    [InlineData(ChartProgramKind.Distribution, "DistributionChart")]
    [InlineData(ChartProgramKind.WeekdayTrend, "WeekdayTrendChart")]
    [InlineData(ChartProgramKind.BarPie, "BarPieChart")]
    public void Factory_ShouldCreateChartConsumerIntentsForCurrentCartesianFamilies(
        ChartProgramKind kind,
        string expectedDeliveryTarget)
    {
        var factory = new AnalyticalIntentFactory();
        var selection = CreateSelection(seriesCount: 2);

        var intent = kind switch
        {
            ChartProgramKind.Main => factory.Main(selection),
            ChartProgramKind.Normalized => factory.Normalized(selection),
            ChartProgramKind.Difference => factory.Difference(selection),
            ChartProgramKind.Ratio => factory.Ratio(selection),
            ChartProgramKind.Distribution => factory.Distribution(selection),
            ChartProgramKind.WeekdayTrend => factory.WeekdayTrend(selection),
            ChartProgramKind.BarPie => factory.BarPie(selection),
            _ => throw new InvalidOperationException()
        };

        Assert.Equal(kind, intent.ProgramRequest.Kind);
        Assert.Equal(ConsumerKind.Chart, intent.Delivery.ConsumerKind);
        Assert.Equal(expectedDeliveryTarget, intent.Delivery.DeliveryTarget);
        Assert.Equal(selection.Signature, intent.Provenance.SourceSignature);
        Assert.Equal("VNext", intent.Provenance.Authority);
        Assert.Equal("Requested", intent.Provenance.TrustClass);
        Assert.Equal(kind, intent.ProgramRequest.Kind);
    }

    [Fact]
    public void SyncfusionSunburst_ShouldCreateHierarchyConsumerIntent()
    {
        var factory = new AnalyticalIntentFactory();
        var selection = CreateSelection(seriesCount: 1);

        var intent = factory.SyncfusionSunburst(selection);

        Assert.Equal(ChartProgramKind.SyncfusionSunburst, intent.ProgramRequest.Kind);
        Assert.Equal(ConsumerKind.HierarchyChart, intent.Delivery.ConsumerKind);
        Assert.Equal("SyncfusionSunburst", intent.Delivery.DeliveryTarget);
    }

    [Fact]
    public void Transform_ShouldPreserveOperationsAndDisplayMode()
    {
        var factory = new AnalyticalIntentFactory();
        var selection = CreateSelection(seriesCount: 2);
        var operations = new[]
        {
            SeriesOperationRequest.Normalize(0, "n", "Normalized"),
            SeriesOperationRequest.Difference(0, 1, "Delta")
        };

        var intent = factory.Transform(selection, "Transform", operations, ChartDisplayMode.Stacked);

        Assert.Equal(ChartProgramKind.Transform, intent.ProgramRequest.Kind);
        Assert.Equal(ChartDisplayMode.Stacked, intent.ProgramRequest.DisplayMode);
        Assert.Equal("Transform", intent.ProgramRequest.TitleOverride);
        Assert.Equal(2, intent.ProgramRequest.SeriesOperations.Count);
        Assert.Equal(AnalyticalCapabilityKind.Transform, intent.Capability.CapabilityKind);
        Assert.Equal(CompositionKind.DerivedSeries, intent.Capability.CompositionKind);
    }

    [Fact]
    public void Create_ShouldPreserveOverlayAndInteractionDeclarations()
    {
        var factory = new AnalyticalIntentFactory();
        var selection = CreateSelection(seriesCount: 1);

        var intent = factory.Main(
            selection,
            overlays: [new OverlayPlan(OverlayKind.AverageLine, "Average")],
            interactions: [new InteractionRequest(InteractionKind.ResetZoom, "MainChart")]);

        Assert.Single(intent.Overlays);
        Assert.Single(intent.Interactions);
        Assert.Contains("Average", intent.Signature, StringComparison.Ordinal);
        Assert.Contains("ResetZoom", intent.Signature, StringComparison.Ordinal);
    }

    private static MetricSelectionRequest CreateSelection(int seriesCount)
    {
        var series = Enumerable.Range(0, seriesCount)
            .Select(index => new MetricSeriesRequest("Weight", $"series-{index + 1}"))
            .ToArray();

        return new MetricSelectionRequest(
            "Weight",
            series,
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 3),
            "HealthMetrics");
    }
}
