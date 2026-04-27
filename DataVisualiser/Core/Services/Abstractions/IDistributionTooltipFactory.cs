using LiveCharts.Wpf;

namespace DataVisualiser.Core.Services.Abstractions;

public interface IDistributionTooltipFactory
{
    IDisposable Create(
        CartesianChart chart,
        Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> tooltipData,
        Dictionary<int, double>? averages);
}
