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
    public void HandleSubtypesLoaded_ShouldReturnApplySelectionStateWhenDataAlreadyLoaded()
    {
        // Syncfusion→Charts tab-switch scenario: HasLoadedData is true (shared viewmodel carries
        // a valid LastContext) and SelectedSeriesCount > 0 and not initializing.
        // The coordinator returns ApplySelectionState — which is why _pendingTabSwitchRestore
        // must be checked before the followUp branches in OnSubtypesLoaded. Without that early
        // check, the standard ApplySelectionState path overwrites the combos with defaulted state
        // before CompleteTabSwitchRestoreAsync can apply the saved selections.
        var coordinator = new ChartHostMetricSelectionCoordinator();

        var result = coordinator.HandleSubtypesLoaded(
            new ChartHostMetricSelectionCoordinator.SubtypesLoadedInput(
                [new MetricNameOption("Weight", "Weight")],
                new MetricNameOption("Weight", "Weight"),
                IsMetricTypeChangePending: false,
                HasLoadedData: true,
                ShouldRefreshDateRangeForCurrentSelection: true,
                IsInitializing: false,
                SelectedSeriesCount: 2),
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

    [Fact]
    public void HandleSubtypesLoaded_ShouldCallUpdateSelectedSubtypesInViewModelBeforeReturningApplySelectionState()
    {
        // This pins the call ordering that MainChartsView depends on:
        // UpdateSelectedSubtypesInViewModel runs synchronously inside HandleSubtypesLoaded,
        // so the delegate passed from CreateSubtypesLoadedActions can suppress it when
        // _pendingTabSwitchRestore is set — preventing the saved selections from being
        // overwritten with the freshly-defaulted combo state before CompleteTabSwitchRestoreAsync runs.
        var coordinator = new ChartHostMetricSelectionCoordinator();
        var calls = new List<string>();

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
                value => calls.Add($"sync:{value}"),
                () => new TestScope(() => calls.Add("suppress:dispose")),
                () => new TestScope(() => calls.Add("batch:dispose")),
                (_, _, _) => calls.Add("refresh-primary"),
                _ => calls.Add("build-dynamic"),
                () => calls.Add("update-selected"),
                _ => { }));

        Assert.Equal(ChartHostMetricSelectionCoordinator.SubtypesFollowUp.ApplySelectionState, result);
        Assert.Contains("update-selected", calls);
        // Must complete before sync is re-enabled (i.e. before the outer sync:False)
        var updateIndex = calls.IndexOf("update-selected");
        var syncOffIndex = calls.LastIndexOf("sync:False");
        Assert.True(updateIndex < syncOffIndex, "UpdateSelectedSubtypesInViewModel must run while sync suppression is still active");
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
