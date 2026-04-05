using DataVisualiser.Shared.Events;
using DataVisualiser.UI.Events;

namespace DataVisualiser.UI.ViewModels;

public interface IMainChartsViewEventSource
{
    event EventHandler<MetricTypesLoadedEventArgs>? MetricTypesLoaded;
    event EventHandler<SubtypesLoadedEventArgs>? SubtypesLoaded;
    event EventHandler<DateRangeLoadedEventArgs>? DateRangeLoaded;
    event EventHandler<DataLoadedEventArgs>? DataLoaded;
    event EventHandler<ChartVisibilityChangedEventArgs>? ChartVisibilityChanged;
    event EventHandler<ErrorEventArgs>? ErrorOccured;
    event EventHandler<ChartUpdateRequestedEventArgs>? ChartUpdateRequested;
    event EventHandler? SelectionStateChanged;
}
