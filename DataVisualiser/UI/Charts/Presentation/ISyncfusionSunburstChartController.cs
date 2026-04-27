using DataVisualiser.UI.Charts.Controllers;

namespace DataVisualiser.UI.Charts.Presentation;

public interface ISyncfusionSunburstChartController : IChartPanelControllerHost
{
    object? ItemsSource { get; set; }
}
