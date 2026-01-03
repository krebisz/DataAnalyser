using DataVisualiser.Charts;

namespace DataVisualiser.ViewModels.Events;

public class DataLoadedEventArgs : EventArgs
{
    public ChartDataContext DataContext { get; set; } = new();
}