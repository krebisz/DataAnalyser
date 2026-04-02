using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.State;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Presentation;

public abstract class CartesianChartControllerAdapterBase<TController> : ChartControllerAdapterBase, ICartesianChartSurface, IWpfCartesianChartHost
    where TController : ICartesianChartControllerHost
{
    protected CartesianChartControllerAdapterBase(TController controller)
        : base(controller)
    {
        Controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    protected TController Controller { get; }

    public virtual CartesianChart Chart => Controller.Chart;

    public override void Clear(ChartState state)
    {
        ChartSurfaceHelper.ClearCartesian(Chart, state);
    }

    public override void ResetZoom()
    {
        ChartSurfaceHelper.ResetZoom(Chart);
    }

    public override bool HasSeries(ChartState state)
    {
        return ChartSurfaceHelper.HasSeries(Chart);
    }
}
