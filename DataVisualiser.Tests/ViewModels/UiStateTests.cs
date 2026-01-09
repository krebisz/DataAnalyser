using DataVisualiser.UI.State;

namespace DataVisualiser.Tests.ViewModels;

public class UiStateTests
{
    [Fact]
    public void PropertyChanged_Raises_WhenValueChanges()
    {
        var uiState = new UiState();
        var observed = new List<string?>();
        uiState.PropertyChanged += (_, e) => observed.Add(e.PropertyName);

        uiState.IsLoadingData = true;

        Assert.Contains(nameof(UiState.IsLoadingData), observed);
    }

    [Fact]
    public void PropertyChanged_DoesNotRaise_WhenValueUnchanged()
    {
        var uiState = new UiState();
        var changeCount = 0;
        uiState.PropertyChanged += (_, _) => changeCount++;

        uiState.IsUiBusy = true;
        uiState.IsUiBusy = true;

        Assert.Equal(1, changeCount);
    }
}
