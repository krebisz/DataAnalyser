using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.Controls;

public interface IChartController
{
    string Key { get; }
    bool RequiresPrimaryData { get; }
    bool RequiresSecondaryData { get; }

    void Initialize();
    Task RenderAsync(ChartDataContext context);
    void Clear(ChartState state);
    void ResetZoom();
    bool HasSeries(ChartState state);
    void UpdateSubtypeOptions();
    void ClearCache();
    void SetVisible(bool isVisible);
    void SetTitle(string? title);
    void SetToggleEnabled(bool isEnabled);
}
