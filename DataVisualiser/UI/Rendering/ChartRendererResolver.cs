using System;
using System.Collections.Generic;
using DataVisualiser.UI.Rendering.ECharts;
using DataVisualiser.UI.Rendering.LiveCharts;

namespace DataVisualiser.UI.Rendering;

public sealed class ChartRendererResolver : IChartRendererResolver
{
    private readonly Dictionary<string, ChartRendererKind> _kindOverrides;
    private readonly IChartRenderer _liveChartsRenderer;
    private readonly IChartRenderer _eChartsRenderer;

    public ChartRendererResolver(Dictionary<string, ChartRendererKind>? kindOverrides = null, IChartRenderer? liveChartsRenderer = null, IChartRenderer? eChartsRenderer = null)
    {
        _kindOverrides = kindOverrides ?? new Dictionary<string, ChartRendererKind>(StringComparer.OrdinalIgnoreCase);
        _liveChartsRenderer = liveChartsRenderer ?? new LiveChartsChartRenderer();
        _eChartsRenderer = eChartsRenderer ?? new EChartsChartRenderer();
    }

    public ChartRendererKind ResolveKind(string chartKey)
    {
        if (string.IsNullOrWhiteSpace(chartKey))
            return ChartRendererKind.LiveCharts;

        return _kindOverrides.TryGetValue(chartKey, out var kind) ? kind : ChartRendererKind.LiveCharts;
    }

    public IChartRenderer ResolveRenderer(string chartKey)
    {
        return ResolveKind(chartKey) switch
        {
            ChartRendererKind.ECharts => _eChartsRenderer,
            _ => _liveChartsRenderer
        };
    }
}
