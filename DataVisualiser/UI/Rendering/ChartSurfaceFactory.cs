using System;
using DataVisualiser.UI.Rendering.ECharts;

namespace DataVisualiser.UI.Rendering;

public sealed class ChartSurfaceFactory : IChartSurfaceFactory
{
    private readonly IChartRendererResolver _rendererResolver;

    public ChartSurfaceFactory(IChartRendererResolver rendererResolver)
    {
        _rendererResolver = rendererResolver ?? throw new ArgumentNullException(nameof(rendererResolver));
    }

    public IChartSurface Create(string chartKey, IChartPanelHost panelHost)
    {
        if (panelHost == null)
            throw new ArgumentNullException(nameof(panelHost));

        return _rendererResolver.ResolveKind(chartKey) switch
        {
            ChartRendererKind.ECharts => new EChartsWebViewSurface(panelHost),
            _ => new ChartPanelSurface(panelHost)
        };
    }
}
