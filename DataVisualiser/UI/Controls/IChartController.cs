using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.Controls;

public interface IChartController
{
    string Key { get; }
    bool RequiresPrimaryData { get; }
    bool RequiresSecondaryData { get; }
    ChartPanelController Panel { get; }
    ButtonBase ToggleButton { get; }

    void Initialize();
    Task RenderAsync(ChartDataContext context);
    void Clear(ChartState state);
    void ResetZoom();
}
