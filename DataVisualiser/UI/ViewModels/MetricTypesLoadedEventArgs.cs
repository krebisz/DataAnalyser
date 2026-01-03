namespace DataVisualiser.UI.ViewModels.Events;

public class MetricTypesLoadedEventArgs : EventArgs
{
    public IEnumerable<string> MetricTypes { get; set; } = Enumerable.Empty<string>();
}