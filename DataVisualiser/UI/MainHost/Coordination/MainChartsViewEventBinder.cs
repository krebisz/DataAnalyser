using DataVisualiser.Shared;
using DataVisualiser.UI.Events;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.MainHost.Coordination;

public sealed class MainChartsViewEventBinder
{
    public sealed record Handlers(
        EventHandler<ChartVisibilityChangedEventArgs> ChartVisibilityChanged,
        EventHandler<ErrorEventArgs> ErrorOccured,
        EventHandler<MetricTypesLoadedEventArgs> MetricTypesLoaded,
        EventHandler<SubtypesLoadedEventArgs> SubtypesLoaded,
        EventHandler<DateRangeLoadedEventArgs> DateRangeLoaded,
        EventHandler<DataLoadedEventArgs> DataLoaded,
        EventHandler<ChartUpdateRequestedEventArgs> ChartUpdateRequested,
        EventHandler SelectionStateChanged);

    private readonly Handlers _handlers;
    private readonly IMainChartsViewEventSource _source;
    private bool _isBound;

    public MainChartsViewEventBinder(IMainChartsViewEventSource source, Handlers handlers)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
    }

    public void Bind()
    {
        if (_isBound)
            return;

        _source.ChartVisibilityChanged += _handlers.ChartVisibilityChanged;
        _source.ErrorOccured += _handlers.ErrorOccured;
        _source.MetricTypesLoaded += _handlers.MetricTypesLoaded;
        _source.SubtypesLoaded += _handlers.SubtypesLoaded;
        _source.DateRangeLoaded += _handlers.DateRangeLoaded;
        _source.DataLoaded += _handlers.DataLoaded;
        _source.ChartUpdateRequested += _handlers.ChartUpdateRequested;
        _source.SelectionStateChanged += _handlers.SelectionStateChanged;
        _isBound = true;
    }

    public void Unbind()
    {
        if (!_isBound)
            return;

        _source.ChartVisibilityChanged -= _handlers.ChartVisibilityChanged;
        _source.ErrorOccured -= _handlers.ErrorOccured;
        _source.MetricTypesLoaded -= _handlers.MetricTypesLoaded;
        _source.SubtypesLoaded -= _handlers.SubtypesLoaded;
        _source.DateRangeLoaded -= _handlers.DateRangeLoaded;
        _source.DataLoaded -= _handlers.DataLoaded;
        _source.ChartUpdateRequested -= _handlers.ChartUpdateRequested;
        _source.SelectionStateChanged -= _handlers.SelectionStateChanged;
        _isBound = false;
    }
}
