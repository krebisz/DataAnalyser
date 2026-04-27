using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Interaction;

public interface IChartTimestampSink
{
    void UpdateChartTimestamps(CartesianChart chart, List<DateTime> timestamps);
}
