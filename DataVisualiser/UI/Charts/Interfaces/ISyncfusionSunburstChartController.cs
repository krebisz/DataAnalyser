using DataVisualiser.UI.Charts.Controllers;

namespace DataVisualiser.UI.Charts.Interfaces;

public interface ISyncfusionSunburstChartController : IChartPanelControllerHost
{
    object? ItemsSource { get; set; }
}
