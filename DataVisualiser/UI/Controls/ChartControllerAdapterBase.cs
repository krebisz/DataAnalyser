using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.Controls;

public abstract class ChartControllerAdapterBase : IChartController, IWpfChartPanelHost
{
    private readonly IChartPanelControllerHost _panelHost;

    protected ChartControllerAdapterBase(IChartPanelControllerHost panelHost)
    {
        _panelHost = panelHost ?? throw new ArgumentNullException(nameof(panelHost));
    }

    public abstract string Key { get; }
    public abstract bool RequiresPrimaryData { get; }
    public abstract bool RequiresSecondaryData { get; }

    public Panel ChartContentPanel => _panelHost.Panel.ChartContentPanel;

    public virtual void Initialize()
    {
    }

    public abstract Task RenderAsync(ChartDataContext context);
    public abstract void Clear(ChartState state);
    public abstract void ResetZoom();
    public abstract bool HasSeries(ChartState state);
    public abstract void UpdateSubtypeOptions();

    public virtual void ClearCache()
    {
    }

    public void SetVisible(bool isVisible)
    {
        _panelHost.Panel.IsChartVisible = isVisible;
    }

    public void SetTitle(string? title)
    {
        _panelHost.Panel.Title = title ?? string.Empty;
    }

    public void SetToggleEnabled(bool isEnabled)
    {
        _panelHost.ToggleButton.IsEnabled = isEnabled;
    }
}
