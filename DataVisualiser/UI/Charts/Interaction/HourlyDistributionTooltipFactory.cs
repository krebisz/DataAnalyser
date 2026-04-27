using DataVisualiser.Core.Services.Abstractions;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Interaction;

public sealed class HourlyDistributionTooltipFactory : IDistributionTooltipFactory
{
    public IDisposable Create(
        CartesianChart chart,
        Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> tooltipData,
        Dictionary<int, double>? averages)
    {
        return new HourlyDistributionTooltip(chart, tooltipData, averages);
    }
}
