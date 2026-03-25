using DataVisualiser.UI.Events;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewEventBinderTests
{
    [Fact]
    public void BindAndUnbind_ShouldAttachHandlersExactlyOnce()
    {
        var source = new FakeEventSource();
        var calls = new List<string>();
        var binder = new MainChartsViewEventBinder(
            source,
            new MainChartsViewEventHandlers(
                (_, _) => calls.Add("visibility"),
                (_, _) => calls.Add("error"),
                (_, _) => calls.Add("metric-types"),
                (_, _) => calls.Add("subtypes"),
                (_, _) => calls.Add("date-range"),
                (_, _) => calls.Add("data"),
                (_, _) => calls.Add("update"),
                (_, _) => calls.Add("selection")));

        binder.Bind();
        binder.Bind();

        source.RaiseAll();
        Assert.Equal(8, calls.Count);

        binder.Unbind();
        source.RaiseAll();
        Assert.Equal(8, calls.Count);
    }

    private sealed class FakeEventSource : IMainChartsViewEventSource
    {
        public event EventHandler<MetricTypesLoadedEventArgs>? MetricTypesLoaded;
        public event EventHandler<SubtypesLoadedEventArgs>? SubtypesLoaded;
        public event EventHandler<DateRangeLoadedEventArgs>? DateRangeLoaded;
        public event EventHandler<DataLoadedEventArgs>? DataLoaded;
        public event EventHandler<ChartVisibilityChangedEventArgs>? ChartVisibilityChanged;
        public event EventHandler<ErrorEventArgs>? ErrorOccured;
        public event EventHandler<ChartUpdateRequestedEventArgs>? ChartUpdateRequested;
        public event EventHandler? SelectionStateChanged;

        public void RaiseAll()
        {
            ChartVisibilityChanged?.Invoke(this, new ChartVisibilityChangedEventArgs { ChartName = "Main", IsVisible = true });
            ErrorOccured?.Invoke(this, new ErrorEventArgs { Message = "error" });
            MetricTypesLoaded?.Invoke(this, new MetricTypesLoadedEventArgs { MetricTypes = [] });
            SubtypesLoaded?.Invoke(this, new SubtypesLoadedEventArgs { Subtypes = [] });
            DateRangeLoaded?.Invoke(this, new DateRangeLoadedEventArgs { MinDate = DateTime.Today, MaxDate = DateTime.Today });
            DataLoaded?.Invoke(this, new DataLoadedEventArgs { DataContext = new DataVisualiser.Core.Orchestration.ChartDataContext() });
            ChartUpdateRequested?.Invoke(this, new ChartUpdateRequestedEventArgs());
            SelectionStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
