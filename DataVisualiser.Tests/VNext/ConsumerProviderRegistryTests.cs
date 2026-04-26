using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Tests.VNext;

public sealed class ConsumerProviderRegistryTests
{
    [Fact]
    public void BuiltIn_ShouldResolveLiveChartsCartesianDelivery()
    {
        var delivery = ConsumerDeliveryContract.Chart(ChartProgramKind.Main, "MainChart");

        var provider = ConsumerProviderRegistry.BuiltIn.Resolve(delivery, ChartRenderPlanKind.Cartesian);

        Assert.Equal("LiveChartsWpf", provider.ProviderKey);
        Assert.Equal(ConsumerKind.Chart, provider.ConsumerKind);
    }

    [Fact]
    public void BuiltIn_ShouldResolveLiveChartsFacetedDelivery()
    {
        var delivery = ConsumerDeliveryContract.Chart(ChartProgramKind.BarPie, "BarPieChart");

        var provider = ConsumerProviderRegistry.BuiltIn.Resolve(delivery, ChartRenderPlanKind.Faceted);

        Assert.Equal("LiveChartsWpf", provider.ProviderKey);
    }

    [Fact]
    public void BuiltIn_ShouldResolveSyncfusionHierarchyDelivery()
    {
        var delivery = ConsumerDeliveryContract.HierarchyChart(ChartProgramKind.SyncfusionSunburst, "SyncfusionSunburst");

        var provider = ConsumerProviderRegistry.BuiltIn.Resolve(delivery, ChartRenderPlanKind.Hierarchy);

        Assert.Equal("SyncfusionSunburst", provider.ProviderKey);
        Assert.Equal(ConsumerKind.HierarchyChart, provider.ConsumerKind);
    }

    [Theory]
    [InlineData(ChartProgramKind.Main)]
    [InlineData(ChartProgramKind.Transform)]
    [InlineData(ChartProgramKind.SyncfusionSunburst)]
    public void BuiltIn_ShouldResolveNonRenderingExportConsumers(ChartProgramKind programKind)
    {
        var delivery = ConsumerDeliveryContract.Export(programKind);

        var provider = ConsumerProviderRegistry.BuiltIn.Resolve(delivery);

        Assert.Equal("EvidenceExport", provider.ProviderKey);
        Assert.Equal(ConsumerKind.Export, provider.ConsumerKind);
    }

    [Fact]
    public void Resolve_ShouldRejectProviderThatCannotSupportPlanKind()
    {
        var delivery = ConsumerDeliveryContract.Chart(ChartProgramKind.Main, "MainChart");

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ConsumerProviderRegistry.BuiltIn.Resolve(delivery, ChartRenderPlanKind.Hierarchy));

        Assert.Contains("No consumer provider supports", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Main", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Hierarchy", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Registry_ShouldRejectDuplicateProviderKeys()
    {
        var first = ConsumerProviderContracts.LiveChartsWpf;
        var duplicate = new ConsumerProviderContract(
            first.ProviderKey,
            "Duplicate",
            first.ConsumerKind,
            first.SupportedProgramKinds,
            first.SupportedPlanKinds);

        var ex = Assert.Throws<ArgumentException>(() => new ConsumerProviderRegistry([first, duplicate]));

        Assert.Contains("Duplicate consumer provider key", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ProviderContract_ShouldSupportCustomThirdPartyChartProvider()
    {
        var provider = new ConsumerProviderContract(
            "ThirdPartyCartesian",
            "Third Party Cartesian",
            ConsumerKind.Chart,
            new HashSet<ChartProgramKind> { ChartProgramKind.Main },
            new HashSet<ChartRenderPlanKind> { ChartRenderPlanKind.Cartesian },
            new Dictionary<string, string> { ["Package"] = "Example.Plugin" });
        var delivery = ConsumerDeliveryContract.Chart(ChartProgramKind.Main, "PluginSurface");

        Assert.True(provider.Supports(delivery, ChartRenderPlanKind.Cartesian));
        Assert.False(provider.Supports(delivery, ChartRenderPlanKind.Hierarchy));
        Assert.Equal("Example.Plugin", provider.Metadata["Package"]);
    }

    [Fact]
    public void ProviderMetadata_ShouldAddBuiltInProviderForKnownDelivery()
    {
        var metadata = new Dictionary<string, string>();
        var added = ChartRenderPlanProviderMetadata.TryAddBuiltInProvider(
            metadata,
            ConsumerDeliveryContract.Chart(ChartProgramKind.Main, "MainChart"),
            ChartProgramKind.Main);

        Assert.True(added);
        Assert.Equal("LiveChartsWpf", metadata[ChartRenderPlanMetadataKeys.ProviderKey]);
        Assert.Equal(ConsumerProviderContracts.LiveChartsWpf.Signature, metadata[ChartRenderPlanMetadataKeys.ProviderSignature]);
    }

    [Fact]
    public void ProviderMetadata_ShouldReturnFalseForUnknownProviderCoverage()
    {
        var metadata = new Dictionary<string, string>();
        var added = ChartRenderPlanProviderMetadata.TryAddBuiltInProvider(
            metadata,
            ConsumerDeliveryContract.Chart(ChartProgramKind.Main, "MainChart"),
            ChartRenderPlanKind.Hierarchy);

        Assert.False(added);
        Assert.False(metadata.ContainsKey(ChartRenderPlanMetadataKeys.ProviderKey));
    }
}
