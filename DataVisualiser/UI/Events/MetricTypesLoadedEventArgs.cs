using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Events;

public class MetricTypesLoadedEventArgs : EventArgs
{
    public IEnumerable<MetricNameOption> MetricTypes { get; set; } = Enumerable.Empty<MetricNameOption>();
}