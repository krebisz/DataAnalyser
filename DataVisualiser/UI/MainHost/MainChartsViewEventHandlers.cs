using DataVisualiser.UI.Events;

namespace DataVisualiser.UI.MainHost;

public sealed record MainChartsViewEventHandlers(
    EventHandler<ChartVisibilityChangedEventArgs> ChartVisibilityChanged,
    EventHandler<ErrorEventArgs> ErrorOccured,
    EventHandler<MetricTypesLoadedEventArgs> MetricTypesLoaded,
    EventHandler<SubtypesLoadedEventArgs> SubtypesLoaded,
    EventHandler<DateRangeLoadedEventArgs> DateRangeLoaded,
    EventHandler<DataLoadedEventArgs> DataLoaded,
    EventHandler<ChartUpdateRequestedEventArgs> ChartUpdateRequested,
    EventHandler SelectionStateChanged);
