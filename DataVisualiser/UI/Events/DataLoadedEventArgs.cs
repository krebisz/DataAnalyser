using DataVisualiser.Core.Orchestration;

namespace DataVisualiser.UI.Events;

public class DataLoadedEventArgs : EventArgs
{
    public ChartDataContext DataContext { get; set; } = new();
}