using DataVisualiser.Core.Orchestration;

namespace DataVisualiser.UI.ViewModels.Events;

public class DataLoadedEventArgs : EventArgs
{
    public ChartDataContext DataContext { get; set; } = new();
}