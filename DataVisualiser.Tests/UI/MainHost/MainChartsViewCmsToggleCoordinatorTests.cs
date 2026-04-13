using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.MainHost;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewCmsToggleCoordinatorTests
{
    [Fact]
    public void SyncStates_ShouldProjectConfigurationAndEnablement()
    {
        var original = CaptureConfiguration();
        try
        {
            CmsConfiguration.UseCmsData = true;
            CmsConfiguration.UseCmsForSingleMetric = true;
            CmsConfiguration.UseCmsForCombinedMetric = false;
            CmsConfiguration.UseCmsForMultiMetric = true;
            CmsConfiguration.UseCmsForNormalized = false;
            CmsConfiguration.UseCmsForWeeklyDistribution = true;
            CmsConfiguration.UseCmsForWeekdayTrend = false;
            CmsConfiguration.UseCmsForHourlyDistribution = true;
            CmsConfiguration.UseCmsForBarPie = false;

            var coordinator = new MainChartsViewCmsToggleCoordinator();
            var checkedCalls = new Dictionary<string, bool>();
            var enabledCalls = new Dictionary<string, bool>();

            coordinator.SyncStates(new MainChartsViewCmsToggleCoordinator.SyncActions(
                value => checkedCalls["cms"] = value,
                value => checkedCalls["single"] = value,
                value => checkedCalls["combined"] = value,
                value => checkedCalls["multi"] = value,
                value => checkedCalls["normalized"] = value,
                value => checkedCalls["weekly"] = value,
                value => checkedCalls["weekday"] = value,
                value => checkedCalls["hourly"] = value,
                value => checkedCalls["barpie"] = value,
                value => enabledCalls["single"] = value,
                value => enabledCalls["combined"] = value,
                value => enabledCalls["multi"] = value,
                value => enabledCalls["normalized"] = value,
                value => enabledCalls["weekly"] = value,
                value => enabledCalls["weekday"] = value,
                value => enabledCalls["hourly"] = value,
                value => enabledCalls["barpie"] = value));

            Assert.True(checkedCalls["cms"]);
            Assert.True(checkedCalls["single"]);
            Assert.False(checkedCalls["combined"]);
            Assert.True(enabledCalls.Values.All(value => value));
        }
        finally
        {
            RestoreConfiguration(original);
        }
    }

    [Fact]
    public async Task HandleCmsToggleChangedAsync_ShouldUpdateConfigurationAndRenderBarPieWhenVisible()
    {
        var original = CaptureConfiguration();
        try
        {
            var coordinator = new MainChartsViewCmsToggleCoordinator();
            var calls = new List<string>();

            await coordinator.HandleCmsToggleChangedAsync(
                isInitializing: false,
                isEnabled: true,
                isBarPieVisible: true,
                new ChartDataContext(),
                new MainChartsViewCmsToggleCoordinator.ChangeActions(
                    enabled => calls.Add($"enabled:{enabled}"),
                    (key, _) =>
                    {
                        calls.Add($"render:{key}");
                        return Task.CompletedTask;
                    }));

            Assert.True(CmsConfiguration.UseCmsData);
            Assert.Equal([$"enabled:True", $"render:{ChartControllerKeys.BarPie}"], calls);
        }
        finally
        {
            RestoreConfiguration(original);
        }
    }

    [Fact]
    public async Task HandleStrategyToggleChangedAsync_ShouldUpdateStrategyFlagsWithoutRenderWhenHidden()
    {
        var original = CaptureConfiguration();
        try
        {
            var coordinator = new MainChartsViewCmsToggleCoordinator();
            var calls = new List<string>();

            await coordinator.HandleStrategyToggleChangedAsync(
                isInitializing: false,
                new MainChartsViewCmsToggleCoordinator.StrategyToggleInput(true, false, true, false, true, false, true, false),
                isBarPieVisible: false,
                context: null,
                new MainChartsViewCmsToggleCoordinator.ChangeActions(
                    _ => calls.Add("enabled"),
                    (key, _) =>
                    {
                        calls.Add($"render:{key}");
                        return Task.CompletedTask;
                    }));

            Assert.True(CmsConfiguration.UseCmsForSingleMetric);
            Assert.False(CmsConfiguration.UseCmsForCombinedMetric);
            Assert.True(CmsConfiguration.UseCmsForMultiMetric);
            Assert.False(CmsConfiguration.UseCmsForNormalized);
            Assert.True(CmsConfiguration.UseCmsForWeeklyDistribution);
            Assert.False(CmsConfiguration.UseCmsForWeekdayTrend);
            Assert.True(CmsConfiguration.UseCmsForHourlyDistribution);
            Assert.False(CmsConfiguration.UseCmsForBarPie);
            Assert.Empty(calls);
        }
        finally
        {
            RestoreConfiguration(original);
        }
    }

    private static CmsState CaptureConfiguration()
    {
        return new CmsState(
            CmsConfiguration.UseCmsData,
            CmsConfiguration.UseCmsForSingleMetric,
            CmsConfiguration.UseCmsForCombinedMetric,
            CmsConfiguration.UseCmsForMultiMetric,
            CmsConfiguration.UseCmsForNormalized,
            CmsConfiguration.UseCmsForWeeklyDistribution,
            CmsConfiguration.UseCmsForWeekdayTrend,
            CmsConfiguration.UseCmsForHourlyDistribution,
            CmsConfiguration.UseCmsForBarPie);
    }

    private static void RestoreConfiguration(CmsState state)
    {
        CmsConfiguration.UseCmsData = state.UseCmsData;
        CmsConfiguration.UseCmsForSingleMetric = state.UseSingleMetric;
        CmsConfiguration.UseCmsForCombinedMetric = state.UseCombinedMetric;
        CmsConfiguration.UseCmsForMultiMetric = state.UseMultiMetric;
        CmsConfiguration.UseCmsForNormalized = state.UseNormalized;
        CmsConfiguration.UseCmsForWeeklyDistribution = state.UseWeeklyDistribution;
        CmsConfiguration.UseCmsForWeekdayTrend = state.UseWeekdayTrend;
        CmsConfiguration.UseCmsForHourlyDistribution = state.UseHourlyDistribution;
        CmsConfiguration.UseCmsForBarPie = state.UseBarPie;
    }

    private sealed record CmsState(
        bool UseCmsData,
        bool UseSingleMetric,
        bool UseCombinedMetric,
        bool UseMultiMetric,
        bool UseNormalized,
        bool UseWeeklyDistribution,
        bool UseWeekdayTrend,
        bool UseHourlyDistribution,
        bool UseBarPie);
}
