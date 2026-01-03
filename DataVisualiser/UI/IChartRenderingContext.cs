using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI;

/// <summary>
///     Provides access to chart rendering context and services that chart panels need.
///     This interface abstracts the dependencies that chart controllers require from MainWindow.
/// </summary>
public interface IChartRenderingContext
{
    ChartDataContext? CurrentDataContext { get; }
    ChartState        ChartState         { get; }

    bool HasSecondaryData(ChartDataContext?   ctx);
    bool ShouldRenderCharts(ChartDataContext? ctx);
}