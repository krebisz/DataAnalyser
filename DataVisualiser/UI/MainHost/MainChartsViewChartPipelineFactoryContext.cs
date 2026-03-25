using DataVisualiser.Core.Rendering.Interaction;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.MainHost;

public sealed record MainChartsViewChartPipelineFactoryContext(
    Dictionary<CartesianChart, List<DateTime>> ChartTimestamps,
    ChartTooltipManager TooltipManager,
    string ConnectionString);
