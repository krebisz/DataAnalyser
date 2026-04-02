using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Events;
using DataVisualiser.UI.SyncfusionViews;

namespace DataVisualiser.Tests.UI.Syncfusion;

public sealed class SyncfusionChartsViewCoordinatorTests
{
    private static readonly ChartDataContext Context = new()
    {
        Data1 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 1m }]
    };

    [Fact]
    public void ShouldRenderAfterSubtypeSelectionChange_IsTrueOnlyForLoadedContext()
    {
        var coordinator = new SyncfusionChartsViewCoordinator();

        Assert.True(coordinator.ShouldRenderAfterSubtypeSelectionChange(false, true, Context));
        Assert.False(coordinator.ShouldRenderAfterSubtypeSelectionChange(true, true, Context));
        Assert.False(coordinator.ShouldRenderAfterSubtypeSelectionChange(false, false, Context));
        Assert.False(coordinator.ShouldRenderAfterSubtypeSelectionChange(false, true, null));
    }

    [Fact]
    public void ShouldRenderAfterVisibilityOnlyToggle_IsTrueOnlyForVisibleSyncfusionToggleWithContext()
    {
        var coordinator = new SyncfusionChartsViewCoordinator();

        Assert.True(coordinator.ShouldRenderAfterVisibilityOnlyToggle(
            new ChartUpdateRequestedEventArgs
            {
                IsVisibilityOnlyToggle = true,
                ToggledChartName = ChartControllerKeys.SyncfusionSunburst,
                ShowSyncfusionSunburst = true
            },
            Context));

        Assert.False(coordinator.ShouldRenderAfterVisibilityOnlyToggle(
            new ChartUpdateRequestedEventArgs
            {
                IsVisibilityOnlyToggle = true,
                ToggledChartName = ChartControllerKeys.SyncfusionSunburst,
                ShowSyncfusionSunburst = false
            },
            Context));
    }

    [Fact]
    public void ShouldRenderWhenViewBecomesVisible_IsTrueOnlyForVisibleActiveChartWithContext()
    {
        var coordinator = new SyncfusionChartsViewCoordinator();

        Assert.True(coordinator.ShouldRenderWhenViewBecomesVisible(false, true, true, Context));
        Assert.False(coordinator.ShouldRenderWhenViewBecomesVisible(true, true, true, Context));
        Assert.False(coordinator.ShouldRenderWhenViewBecomesVisible(false, false, true, Context));
        Assert.False(coordinator.ShouldRenderWhenViewBecomesVisible(false, true, false, Context));
        Assert.False(coordinator.ShouldRenderWhenViewBecomesVisible(false, true, true, null));
    }

    [Fact]
    public void GetReachabilityExportMessage_ReturnsExplicitCurrentStatus()
    {
        var coordinator = new SyncfusionChartsViewCoordinator();

        Assert.Equal(SyncfusionChartsViewCoordinator.ReachabilityExportNotWiredMessage, coordinator.GetReachabilityExportMessage());
    }

    [Theory]
    [InlineData(ChartControllerKeys.SyncfusionSunburst, true)]
    [InlineData("syncfusionsunburst", true)]
    [InlineData(ChartControllerKeys.Main, false)]
    [InlineData("", false)]
    public void IsRegisteredKey_MatchesOnlyManagedSyncfusionChart(string key, bool expected)
    {
        Assert.Equal(expected, SyncfusionChartsViewCoordinator.IsRegisteredKey(key));
    }
}
