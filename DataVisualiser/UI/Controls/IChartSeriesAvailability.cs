using DataVisualiser.UI.State;

namespace DataVisualiser.UI.Controls;

public interface IChartSeriesAvailability
{
    bool HasSeries(ChartState state);
}