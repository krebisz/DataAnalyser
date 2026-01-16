namespace DataVisualiser.UI.Events;

using DataVisualiser.Shared.Models;

public class SubtypesLoadedEventArgs : EventArgs
{
    public IEnumerable<MetricNameOption> Subtypes { get; set; } = Enumerable.Empty<MetricNameOption>();
}
