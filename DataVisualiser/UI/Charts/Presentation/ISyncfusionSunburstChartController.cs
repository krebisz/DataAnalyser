using DataVisualiser.Core.Rendering.Syncfusion;
using DataVisualiser.UI.Charts.Controllers;

namespace DataVisualiser.UI.Charts.Presentation;

public interface ISyncfusionSunburstChartController : IChartPanelControllerHost, ISyncfusionSunburstRenderTarget
{
    object? ItemsSource { get; set; }
}
