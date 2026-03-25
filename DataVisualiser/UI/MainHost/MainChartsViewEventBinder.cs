using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsViewEventBinder
{
    private readonly MainChartsViewEventHandlers _handlers;
    private readonly IMainChartsViewEventSource _source;
    private bool _isBound;

    public MainChartsViewEventBinder(IMainChartsViewEventSource source, MainChartsViewEventHandlers handlers)
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
