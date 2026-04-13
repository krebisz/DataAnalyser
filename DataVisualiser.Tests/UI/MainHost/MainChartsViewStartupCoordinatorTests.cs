using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Coordination;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewStartupCoordinatorTests
{
    [Fact]
    public void Execute_ShouldInvokeStartupActionsInExpectedOrder()
    {
        var calls = new List<string>();
        var coordinator = new MainChartsViewStartupCoordinator();

        coordinator.Execute(new MainChartsViewStartupCoordinator.Actions(
            () => calls.Add("date"),
            () => calls.Add("default-ui"),
            () => calls.Add("selector"),
            () => calls.Add("resolution"),
            () => calls.Add("charts"),
            () => calls.Add("request-update"),
            () => calls.Add("sync-cms"),
            () => calls.Add("complete-init"),
            () => calls.Add("sync-buttons")));

        Assert.Equal(
            ["date", "default-ui", "selector", "resolution", "charts", "request-update", "sync-cms", "complete-init", "sync-buttons"],
            calls);
    }
}
