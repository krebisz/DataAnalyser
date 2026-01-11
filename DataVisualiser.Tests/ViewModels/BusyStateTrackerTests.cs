using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.ViewModels;

public class BusyStateTrackerTests
{
    [Fact]
    public void IsBusy_TracksUiStateChanges()
    {
        var uiState = new UiState();
        var tracker = new BusyStateTracker(uiState);
        var changeCount = 0;
        tracker.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(BusyStateTracker.IsBusy))
                changeCount++;
        };

        uiState.IsLoadingData = true;

        Assert.True(tracker.IsBusy);
        Assert.Equal(1, changeCount);
    }

    [Fact]
    public void IsBusy_IgnoresUnrelatedProperties()
    {
        var uiState = new UiState();
        var tracker = new BusyStateTracker(uiState);
        var changeCount = 0;
        tracker.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(BusyStateTracker.IsBusy))
                changeCount++;
        };

        uiState.DynamicSubtypeCount = 2;

        Assert.False(tracker.IsBusy);
        Assert.Equal(0, changeCount);
    }
}
