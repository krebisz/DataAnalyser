using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI;

/// <summary>
///     Adapter that implements IChartRenderingContext, providing access to MainWindow's chart rendering capabilities.
///     This allows chart panel controllers to access data and services without direct coupling to MainWindow.
/// </summary>
public class ChartRenderingContextAdapter : IChartRenderingContext
{
    private readonly Func<ChartDataContext?>       _getCurrentContext;
    private readonly Func<ChartDataContext?, bool> _hasSecondaryData;
    private readonly Func<ChartDataContext?, bool> _shouldRenderCharts;

    public ChartRenderingContextAdapter(ChartState chartState, Func<ChartDataContext?> getCurrentContext, Func<ChartDataContext?, bool> hasSecondaryData, Func<ChartDataContext?, bool> shouldRenderCharts)
    {
        ChartState = chartState;
        _getCurrentContext = getCurrentContext;
        _hasSecondaryData = hasSecondaryData;
        _shouldRenderCharts = shouldRenderCharts;
    }

    public ChartDataContext? CurrentDataContext => _getCurrentContext();
    public ChartState        ChartState         { get; }

    public bool HasSecondaryData(ChartDataContext? ctx)
    {
        return _hasSecondaryData(ctx ?? CurrentDataContext);
    }

    public bool ShouldRenderCharts(ChartDataContext? ctx)
    {
        return _shouldRenderCharts(ctx ?? CurrentDataContext);
    }
}