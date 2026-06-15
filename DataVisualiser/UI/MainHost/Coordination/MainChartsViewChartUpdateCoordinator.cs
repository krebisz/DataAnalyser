using System.Diagnostics;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Events;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost.Coordination;

public sealed class MainChartsViewChartUpdateCoordinator
{
    private static readonly IReadOnlyList<ChartRenderPolicy> RenderPolicies =
    [
        new(ChartControllerKeys.Main, state => state.IsMainVisible),
        new(ChartControllerKeys.Normalized, state => state.IsNormalizedVisible, RequiresSecondaryData: true),
        new(ChartControllerKeys.DiffRatio, state => state.IsDiffRatioVisible, RequiresSecondaryData: true),
        new(ChartControllerKeys.Distribution, state => state.IsDistributionVisible),
        new(ChartControllerKeys.WeeklyTrend, state => state.IsWeeklyTrendVisible),
        new(ChartControllerKeys.Transform, state => state.IsTransformPanelVisible),
        new(ChartControllerKeys.BarPie, state => state.IsBarPieVisible)
    ];

    public sealed class Actions(
        Action<string, bool> setChartVisibility,
        Action updateDistributionChartTypeVisibility,
        Action updateWeekdayTrendChartTypeVisibility,
        Action<ChartDataContext?> handleTransformVisibilityOnlyToggle,
        Func<string, ChartDataContext, Task> renderChartAsync,
        Action<string> clearChart)
    {
        public Action<string, bool> SetChartVisibility { get; } = setChartVisibility ?? throw new ArgumentNullException(nameof(setChartVisibility));
        public Action UpdateDistributionChartTypeVisibility { get; } = updateDistributionChartTypeVisibility ?? throw new ArgumentNullException(nameof(updateDistributionChartTypeVisibility));
        public Action UpdateWeekdayTrendChartTypeVisibility { get; } = updateWeekdayTrendChartTypeVisibility ?? throw new ArgumentNullException(nameof(updateWeekdayTrendChartTypeVisibility));
        public Action<ChartDataContext?> HandleTransformVisibilityOnlyToggle { get; } = handleTransformVisibilityOnlyToggle ?? throw new ArgumentNullException(nameof(handleTransformVisibilityOnlyToggle));
        public Func<string, ChartDataContext, Task> RenderChartAsync { get; } = renderChartAsync ?? throw new ArgumentNullException(nameof(renderChartAsync));
        public Action<string> ClearChart { get; } = clearChart ?? throw new ArgumentNullException(nameof(clearChart));
    }

    public async Task<bool> TryHandleVisibilityOnlyToggleAsync(ChartUpdateRequestedEventArgs args, ChartState chartState, ChartDataContext? context, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(chartState);
        ArgumentNullException.ThrowIfNull(actions);

        if (!args.IsVisibilityOnlyToggle || string.IsNullOrWhiteSpace(args.ToggledChartName))
            return false;

        if (!IsKnownChartKey(args.ToggledChartName))
        {
            Debug.WriteLine($"[ChartRegistry] Ignoring visibility-only toggle for unknown chart '{args.ToggledChartName}'.");
            return true;
        }

        ApplyVisibilityForToggle(args, actions);

        if (args.ToggledChartName == ChartControllerKeys.Transform)
        {
            actions.HandleTransformVisibilityOnlyToggle(context);
            return true;
        }

        if (!IsChartVisible(chartState, args.ToggledChartName) || !ShouldRenderCharts(context))
            return true;

        await RenderSingleChartAsync(chartState, args.ToggledChartName, context!, actions);
        return true;
    }

    public void ApplyAllChartVisibilities(ChartUpdateRequestedEventArgs args, ChartState chartState, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(chartState);
        ArgumentNullException.ThrowIfNull(actions);

        actions.SetChartVisibility(ChartControllerKeys.Main, args.ShowMain);
        actions.SetChartVisibility(ChartControllerKeys.Normalized, args.ShowNormalized);
        actions.SetChartVisibility(ChartControllerKeys.DiffRatio, args.ShowDiffRatio);
        actions.SetChartVisibility(ChartControllerKeys.Distribution, args.ShowDistribution);
        actions.UpdateDistributionChartTypeVisibility();
        actions.SetChartVisibility(ChartControllerKeys.WeeklyTrend, args.ShowWeeklyTrend);
        actions.UpdateWeekdayTrendChartTypeVisibility();
        actions.SetChartVisibility(ChartControllerKeys.Transform, chartState.IsTransformPanelVisible);
        actions.SetChartVisibility(ChartControllerKeys.BarPie, args.ShowBarPie);
    }

    public async Task RenderVisibleChartsAsync(ChartState chartState, ChartDataContext? context, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(chartState);
        ArgumentNullException.ThrowIfNull(actions);

        if (!ShouldRenderCharts(context))
            return;

        var safeContext = context!;
        var hasSecondaryData = HasSecondaryData(safeContext);

        if (!hasSecondaryData)
            ClearSecondaryDataCharts(actions);

        foreach (var policy in RenderPolicies)
        {
            if (!ShouldRenderPolicy(policy, chartState, hasSecondaryData))
                continue;

            await actions.RenderChartAsync(policy.ChartKey, safeContext);
        }
    }

    public static bool ShouldRestoreChartsWhenViewLoads(bool isInitializing, ChartDataContext? context)
    {
        return !isInitializing && ShouldRenderCharts(context);
    }

    public async Task RenderSingleChartAsync(ChartState chartState, string chartName, ChartDataContext context, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(chartState);
        ArgumentException.ThrowIfNullOrWhiteSpace(chartName);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(actions);

        var policy = FindRenderPolicy(chartName);
        if (policy != null && ShouldRenderPolicy(policy, chartState, HasSecondaryData(context)))
        {
            await actions.RenderChartAsync(policy.ChartKey, context);
            return;
        }

        Debug.WriteLine($"[ChartRegistry] RenderSingleChart called with unknown or hidden key '{chartName}'.");
    }

    public static bool ShouldRenderCharts(ChartDataContext? context)
    {
        return context != null && context.Data1 != null && context.Data1.Any();
    }

    private static void ApplyVisibilityForToggle(ChartUpdateRequestedEventArgs args, Actions actions)
    {
        switch (args.ToggledChartName)
        {
            case ChartControllerKeys.Main:
                actions.SetChartVisibility(ChartControllerKeys.Main, args.ShowMain);
                break;
            case ChartControllerKeys.Normalized:
                actions.SetChartVisibility(ChartControllerKeys.Normalized, args.ShowNormalized);
                break;
            case ChartControllerKeys.DiffRatio:
                actions.SetChartVisibility(ChartControllerKeys.DiffRatio, args.ShowDiffRatio);
                break;
            case ChartControllerKeys.Distribution:
                actions.SetChartVisibility(ChartControllerKeys.Distribution, args.ShowDistribution);
                actions.UpdateDistributionChartTypeVisibility();
                break;
            case ChartControllerKeys.WeeklyTrend:
                actions.SetChartVisibility(ChartControllerKeys.WeeklyTrend, args.ShowWeeklyTrend);
                actions.UpdateWeekdayTrendChartTypeVisibility();
                break;
            case ChartControllerKeys.Transform:
                actions.SetChartVisibility(ChartControllerKeys.Transform, args.ShowTransformPanel);
                break;
            case ChartControllerKeys.BarPie:
                actions.SetChartVisibility(ChartControllerKeys.BarPie, args.ShowBarPie);
                break;
        }
    }

    private static bool IsKnownChartKey(string key)
    {
        return ChartControllerKeys.All.Any(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsChartVisible(ChartState chartState, string chartName)
    {
        return FindRenderPolicy(chartName)?.IsVisible(chartState) ?? false;
    }

    private static bool HasSecondaryData(ChartDataContext context)
    {
        return context.Data2 != null && context.Data2.Any();
    }

    private static ChartRenderPolicy? FindRenderPolicy(string chartName)
    {
        return RenderPolicies.FirstOrDefault(policy => string.Equals(policy.ChartKey, chartName, StringComparison.Ordinal));
    }

    private static bool ShouldRenderPolicy(ChartRenderPolicy policy, ChartState chartState, bool hasSecondaryData)
    {
        return policy.IsVisible(chartState) && (!policy.RequiresSecondaryData || hasSecondaryData);
    }

    private static void ClearSecondaryDataCharts(Actions actions)
    {
        foreach (var policy in RenderPolicies.Where(policy => policy.RequiresSecondaryData))
            actions.ClearChart(policy.ChartKey);
    }


    private static void ClearExtendedCharts(ChartState chartState, Actions actions)
    {
        foreach (var policy in RenderPolicies.Where(policy => policy.ChartKey != ChartControllerKeys.Main && policy.IsVisible(chartState)))
            actions.ClearChart(policy.ChartKey);
    }

    private sealed record ChartRenderPolicy(string ChartKey, Func<ChartState, bool> IsVisible, bool RequiresSecondaryData = false);
}
