using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Coordination;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class ChartHostMetricSelectionCoordinatorTests
{
    [Fact]
    public void HandleMetricTypesLoaded_ShouldAddAllOptionSelectFirstMetricAndLoadSubtypes()
    {
        var coordinator = new ChartHostMetricSelectionCoordinator();
        var items = new List<MetricNameOption>();
        var calls = new List<string>();
        var selectedIndex = -1;
        var selectedMetricType = "BodyFat";

        coordinator.HandleMetricTypesLoaded(
            [new MetricNameOption("Weight", "Weight"), new MetricNameOption("BodyFat", "BodyFat")],
            new ChartHostMetricSelectionCoordinator.MetricTypesLoadedActions(
                () => items.Clear(),
                option => items.Add(option),
                () => items.Count,
                value => calls.Add(value ? "sync:on" : "sync:off"),
                () => new TestScope(() => calls.Add("batch:dispose")),
                index =>
                {
                    selectedIndex = index;
                    calls.Add($"index:{index}");
                },
                () => selectedMetricType,
                value =>
                {
                    selectedMetricType = value;
                    calls.Add($"metric:{value}");
                },
                () => calls.Add("load-subtypes"),
                () => calls.Add("clear-primary"),
                value => calls.Add($"primary-enabled:{value}"),
                () => calls.Add("clear-dynamic")));

        Assert.Equal(["(All)", "Weight", "BodyFat"], items.Select(item => item.Value));
        Assert.Equal(1, selectedIndex);
        Assert.Contains("load-subtypes", calls);
        Assert.DoesNotContain("clear-primary", calls);
    }

    [Fact]
    public void HandleMetricTypesLoaded_ShouldClearSubtypeControlsWhenNoMetricTypesExist()
    {
        var coordinator = new ChartHostMetricSelectionCoordinator();
        var calls = new List<string>();

        coordinator.HandleMetricTypesLoaded(
            [],
            new ChartHostMetricSelectionCoordinator.MetricTypesLoadedActions(
                () => calls.Add("clear-metrics"),
                _ => calls.Add("add-metric"),
                () => 0,
                value => calls.Add($"sync:{value}"),
                () => new TestScope(() => calls.Add("batch:dispose")),
                _ => calls.Add("set-index"),
                () => null,
                _ => calls.Add("set-selected"),
                () => calls.Add("load-subtypes"),
                () => calls.Add("clear-primary"),
                value => calls.Add($"primary-enabled:{value}"),
                () => calls.Add("clear-dynamic")));

        Assert.Equal(["clear-metrics", "clear-primary", "primary-enabled:False", "clear-dynamic"], calls);
    }

    [Fact]
    public void HandleMetricTypeSelectionChanged_ShouldResetStateAndLoadSubtypes()
    {
        var coordinator = new ChartHostMetricSelectionCoordinator();
        var calls = new List<string>();

        coordinator.HandleMetricTypeSelectionChanged(
            "Weight",
            new ChartHostMetricSelectionCoordinator.MetricTypeSelectionChangedActions(
                value => calls.Add($"pending:{value}"),
                value => calls.Add($"sync:{value}"),
                () => new TestScope(() => calls.Add("batch:dispose")),
                () => calls.Add("reset"),
                value => calls.Add($"metric:{value}"),
                () => calls.Add("clear-subtypes"),
                () => calls.Add("update-selected"),
                () => calls.Add("load-subtypes")));

        Assert.Equal(
            ["pending:True", "sync:True", "reset", "metric:Weight", "clear-subtypes", "update-selected", "batch:dispose", "sync:False", "load-subtypes"],
            calls);
    }

    [Fact]
    public void HandleSubtypesLoaded_ShouldReturnLoadDateRangeWhenMetricTypeChangePending()
    {
        var coordinator = new ChartHostMetricSelectionCoordinator();
        var calls = new List<string>();

        var result = coordinator.HandleSubtypesLoaded(
            new ChartHostMetricSelectionCoordinator.SubtypesLoadedInput(
                [new MetricNameOption("Weight", "Weight")],
                new MetricNameOption("Weight", "Weight"),
                IsMetricTypeChangePending: true,
                HasLoadedData: false,
                ShouldRefreshDateRangeForCurrentSelection: true,
                IsInitializing: false,
                SelectedSeriesCount: 1),
            new ChartHostMetricSelectionCoordinator.SubtypesLoadedActions(
                value => calls.Add($"sync:{value}"),
                () => new TestScope(() => calls.Add("suppress:dispose")),
                () => new TestScope(() => calls.Add("batch:dispose")),
                (_, _, _) => calls.Add("refresh-primary"),
                _ => calls.Add("build-dynamic"),
                () => calls.Add("update-selected"),
                value => calls.Add($"pending:{value}")));

        Assert.Equal(ChartHostMetricSelectionCoordinator.SubtypesFollowUp.LoadDateRange, result);
        Assert.Contains("pending:False", calls);
    }

    [Fact]
    public void HandleSubtypesLoaded_ShouldReturnApplySelectionStateWhenSelectionExistsAndNotInitializing()
    {
        var coordinator = new ChartHostMetricSelectionCoordinator();

        var result = coordinator.HandleSubtypesLoaded(
            new ChartHostMetricSelectionCoordinator.SubtypesLoadedInput(
                [new MetricNameOption("Weight", "Weight")],
                new MetricNameOption("Weight", "Weight"),
                IsMetricTypeChangePending: false,
                HasLoadedData: false,
                ShouldRefreshDateRangeForCurrentSelection: false,
                IsInitializing: false,
                SelectedSeriesCount: 1),
            new ChartHostMetricSelectionCoordinator.SubtypesLoadedActions(
                _ => { },
                () => new TestScope(() => { }),
                () => new TestScope(() => { }),
                (_, _, _) => { },
                _ => { },
                () => { },
                _ => { }));

        Assert.Equal(ChartHostMetricSelectionCoordinator.SubtypesFollowUp.ApplySelectionState, result);
    }

    private sealed class TestScope : IDisposable
    {
        private readonly Action _onDispose;

        public TestScope(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose();
        }
    }
}
