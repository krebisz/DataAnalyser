using System.Collections.Generic;
using DataVisualiser.UI.Charts.Presentation.ECharts;
using DataVisualiser.UI.Charts.Presentation.LiveCharts;

using DataVisualiser.UI.Charts.Presentation;
namespace DataVisualiser.Tests.Core.Rendering;

public sealed class ChartRendererResolverTests
{
    [Fact]
    public void ResolveKind_DefaultsToLiveCharts_ForUnknownKey()
    {
        var resolver = new ChartRendererResolver();

        var kind = resolver.ResolveKind("UnknownChart");

        Assert.Equal(ChartRendererKind.LiveCharts, kind);
    }

    [Fact]
    public void ResolveKind_UsesOverride_WhenProvided()
    {
        var overrides = new Dictionary<string, ChartRendererKind>
        {
            [ChartControllerKeys.BarPie] = ChartRendererKind.ECharts
        };
        var resolver = new ChartRendererResolver(overrides);

        var kind = resolver.ResolveKind(ChartControllerKeys.BarPie);

        Assert.Equal(ChartRendererKind.ECharts, kind);
    }

    [Fact]
    public void ResolveRenderer_ReturnsEChartsRenderer_ForEChartsOverride()
    {
        var overrides = new Dictionary<string, ChartRendererKind>
        {
            ["Treemap"] = ChartRendererKind.ECharts
        };
        var resolver = new ChartRendererResolver(overrides);

        var renderer = resolver.ResolveRenderer("Treemap");

        Assert.IsType<EChartsChartRenderer>(renderer);
    }

    [Fact]
    public void ResolveRenderer_ReturnsLiveChartsRenderer_ByDefault()
    {
        var resolver = new ChartRendererResolver();

        var renderer = resolver.ResolveRenderer("AnyChart");

        Assert.IsType<LiveChartsChartRenderer>(renderer);
    }
}
