using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Events;

public class SubtypesLoadedEventArgs : EventArgs
{
    public IEnumerable<MetricNameOption> Subtypes { get; set; } = Enumerable.Empty<MetricNameOption>();
}