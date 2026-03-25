using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.UI.MainHost;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewResolutionResetCoordinatorTests
{
    [Fact]
    public void ExecuteReset_ShouldUsePassedResolutionValue_AndRequestReload()
    {
        var calls = new List<string>();
        string? tableName = null;
        var coordinator = new MainChartsViewResolutionResetCoordinator();

        coordinator.ExecuteReset(
            "All",
            new MainChartsViewResolutionResetActions(
                () => calls.Add("start"),
                () => calls.Add("clear-charts"),
                () => calls.Add("clear-metric"),
                () => calls.Add("clear-context"),
                () => calls.Add("clear-metric-items"),
                () => calls.Add("clear-dynamic"),
                () => calls.Add("clear-subtypes"),
                () => calls.Add("disable-subtype"),
                value =>
                {
                    tableName = value;
                    calls.Add($"table:{value}");
                },
                () => calls.Add("reload"),
                count => calls.Add($"primary:{count}"),
                count => calls.Add($"secondary:{count}")));

        Assert.Equal(DataAccessDefaults.DefaultTableName, tableName);
        Assert.Contains("reload", calls);
        Assert.Equal(
            ["start", "clear-charts", "clear-metric", "clear-context", "clear-metric-items", "clear-dynamic", "clear-subtypes", "disable-subtype", $"table:{DataAccessDefaults.DefaultTableName}", "reload", "primary:0", "secondary:0"],
            calls);
    }

    [Fact]
    public void HandleSuppressedError_ShouldClearFlagAndCharts()
    {
        var calls = new List<string>();
        var coordinator = new MainChartsViewResolutionResetCoordinator();

        coordinator.HandleSuppressedError(
            () => calls.Add("clear-flag"),
            () => calls.Add("clear-charts"));

        Assert.Equal(["clear-flag", "clear-charts"], calls);
    }
}
