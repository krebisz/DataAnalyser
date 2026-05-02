using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Events;

public class DataLoadedEventArgs : EventArgs
{
    public ChartDataContext DataContext { get; set; } = new();
}

public class DateRangeLoadedEventArgs : EventArgs
{
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
}

public class MetricTypesLoadedEventArgs : EventArgs
{
    public IEnumerable<MetricNameOption> MetricTypes { get; set; } = Enumerable.Empty<MetricNameOption>();
}

public class SubtypesLoadedEventArgs : EventArgs
{
    public IEnumerable<MetricNameOption> Subtypes { get; set; } = Enumerable.Empty<MetricNameOption>();
}
