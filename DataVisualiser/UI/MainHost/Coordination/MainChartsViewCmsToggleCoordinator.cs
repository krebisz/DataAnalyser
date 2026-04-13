using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.UI.MainHost.Coordination;

public sealed class MainChartsViewCmsToggleCoordinator
{
    public sealed record SyncActions(
        Action<bool> SetCmsEnabledChecked,
        Action<bool> SetSingleChecked,
        Action<bool> SetCombinedChecked,
        Action<bool> SetMultiChecked,
        Action<bool> SetNormalizedChecked,
        Action<bool> SetWeeklyChecked,
        Action<bool> SetWeekdayTrendChecked,
        Action<bool> SetHourlyChecked,
        Action<bool> SetBarPieChecked,
        Action<bool> SetSingleEnabled,
        Action<bool> SetCombinedEnabled,
        Action<bool> SetMultiEnabled,
        Action<bool> SetNormalizedEnabled,
        Action<bool> SetWeeklyEnabled,
        Action<bool> SetWeekdayTrendEnabled,
        Action<bool> SetHourlyEnabled,
        Action<bool> SetBarPieEnabled);

    public sealed record StrategyToggleInput(
        bool UseSingleMetric,
        bool UseCombinedMetric,
        bool UseMultiMetric,
        bool UseNormalized,
        bool UseWeeklyDistribution,
        bool UseWeekdayTrend,
        bool UseHourlyDistribution,
        bool UseBarPie);

    public sealed record ChangeActions(
        Action<bool> UpdateToggleEnablement,
        Func<string, ChartDataContext, Task> RenderChartAsync);

    public void SyncStates(SyncActions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        actions.SetCmsEnabledChecked(CmsConfiguration.UseCmsData);
        actions.SetSingleChecked(CmsConfiguration.UseCmsForSingleMetric);
        actions.SetCombinedChecked(CmsConfiguration.UseCmsForCombinedMetric);
        actions.SetMultiChecked(CmsConfiguration.UseCmsForMultiMetric);
        actions.SetNormalizedChecked(CmsConfiguration.UseCmsForNormalized);
        actions.SetWeeklyChecked(CmsConfiguration.UseCmsForWeeklyDistribution);
        actions.SetWeekdayTrendChecked(CmsConfiguration.UseCmsForWeekdayTrend);
        actions.SetHourlyChecked(CmsConfiguration.UseCmsForHourlyDistribution);
        actions.SetBarPieChecked(CmsConfiguration.UseCmsForBarPie);

        UpdateEnablement(CmsConfiguration.UseCmsData, actions);
    }

    public async Task HandleCmsToggleChangedAsync(bool isInitializing, bool isEnabled, bool isBarPieVisible, ChartDataContext? context, ChangeActions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        if (isInitializing)
            return;

        CmsConfiguration.UseCmsData = isEnabled;
        actions.UpdateToggleEnablement(isEnabled);

        if (isBarPieVisible)
            await actions.RenderChartAsync(ChartControllerKeys.BarPie, context ?? new ChartDataContext());
    }

    public async Task HandleStrategyToggleChangedAsync(bool isInitializing, StrategyToggleInput input, bool isBarPieVisible, ChartDataContext? context, ChangeActions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        if (isInitializing)
            return;

        CmsConfiguration.UseCmsForSingleMetric = input.UseSingleMetric;
        CmsConfiguration.UseCmsForCombinedMetric = input.UseCombinedMetric;
        CmsConfiguration.UseCmsForMultiMetric = input.UseMultiMetric;
        CmsConfiguration.UseCmsForNormalized = input.UseNormalized;
        CmsConfiguration.UseCmsForWeeklyDistribution = input.UseWeeklyDistribution;
        CmsConfiguration.UseCmsForWeekdayTrend = input.UseWeekdayTrend;
        CmsConfiguration.UseCmsForHourlyDistribution = input.UseHourlyDistribution;
        CmsConfiguration.UseCmsForBarPie = input.UseBarPie;

        if (isBarPieVisible)
            await actions.RenderChartAsync(ChartControllerKeys.BarPie, context ?? new ChartDataContext());
    }

    private static void UpdateEnablement(bool enabled, SyncActions actions)
    {
        actions.SetSingleEnabled(enabled);
        actions.SetCombinedEnabled(enabled);
        actions.SetMultiEnabled(enabled);
        actions.SetNormalizedEnabled(enabled);
        actions.SetWeeklyEnabled(enabled);
        actions.SetWeekdayTrendEnabled(enabled);
        actions.SetHourlyEnabled(enabled);
        actions.SetBarPieEnabled(enabled);
    }
}
