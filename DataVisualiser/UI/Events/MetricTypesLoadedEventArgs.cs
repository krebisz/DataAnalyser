namespace DataVisualiser.UI.Events;

using DataVisualiser.Shared.Models;

public class MetricTypesLoadedEventArgs : EventArgs
{
    public IEnumerable<MetricNameOption> MetricTypes { get; set; } = Enumerable.Empty<MetricNameOption>();
}
