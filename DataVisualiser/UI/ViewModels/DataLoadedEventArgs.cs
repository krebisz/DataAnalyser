using DataVisualiser.Core.Orchestration;

namespace DataVisualiser.UI.ViewModels;

public class DataLoadedEventArgs : EventArgs
{
    public ChartDataContext DataContext { get; set; } = new();
}