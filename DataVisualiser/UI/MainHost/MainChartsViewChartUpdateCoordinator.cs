using System.Diagnostics;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Events;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsViewChartUpdateCoordinator
{
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

        if (chartState.IsMainVisible)
            await actions.RenderChartAsync(ChartControllerKeys.Main, safeContext);

        if (hasSecondaryData)
        {
            if (chartState.IsNormalizedVisible)
                await actions.RenderChartAsync(ChartControllerKeys.Normalized, safeContext);

            if (chartState.IsDiffRatioVisible)
                await actions.RenderChartAsync(ChartControllerKeys.DiffRatio, safeContext);
        }
        else
        {
            actions.ClearChart(ChartControllerKeys.Normalized);
            actions.ClearChart(ChartControllerKeys.DiffRatio);
        }

        if (chartState.IsDistributionVisible)
            await actions.RenderChartAsync(ChartControllerKeys.Distribution, safeContext);

        if (chartState.IsWeeklyTrendVisible)
            await actions.RenderChartAsync(ChartControllerKeys.WeeklyTrend, safeContext);

        if (chartState.IsTransformPanelVisible)
            await actions.RenderChartAsync(ChartControllerKeys.Transform, safeContext);

        if (chartState.IsBarPieVisible)
            await actions.RenderChartAsync(ChartControllerKeys.BarPie, safeContext);
    }

    public async Task RenderSingleChartAsync(ChartState chartState, string chartName, ChartDataContext context, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(chartState);
        ArgumentException.ThrowIfNullOrWhiteSpace(chartName);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(actions);

        var hasSecondaryData = HasSecondaryData(context);

        switch (chartName)
        {
            case ChartControllerKeys.Main when chartState.IsMainVisible:
                await actions.RenderChartAsync(ChartControllerKeys.Main, context);
                break;
            case ChartControllerKeys.Normalized when chartState.IsNormalizedVisible && hasSecondaryData:
                await actions.RenderChartAsync(ChartControllerKeys.Normalized, context);
                break;
            case ChartControllerKeys.DiffRatio when chartState.IsDiffRatioVisible && hasSecondaryData:
                await actions.RenderChartAsync(ChartControllerKeys.DiffRatio, context);
                break;
            case ChartControllerKeys.Distribution when chartState.IsDistributionVisible:
                await actions.RenderChartAsync(ChartControllerKeys.Distribution, context);
                break;
            case ChartControllerKeys.WeeklyTrend when chartState.IsWeeklyTrendVisible:
                await actions.RenderChartAsync(ChartControllerKeys.WeeklyTrend, context);
                break;
            case ChartControllerKeys.Transform when chartState.IsTransformPanelVisible:
                await actions.RenderChartAsync(ChartControllerKeys.Transform, context);
                break;
            case ChartControllerKeys.BarPie when chartState.IsBarPieVisible:
                await actions.RenderChartAsync(ChartControllerKeys.BarPie, context);
                break;
            default:
                Debug.WriteLine($"[ChartRegistry] RenderSingleChart called with unknown or hidden key '{chartName}'.");
                break;
        }
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
        return chartName switch
        {
            ChartControllerKeys.Main => chartState.IsMainVisible,
            ChartControllerKeys.Normalized => chartState.IsNormalizedVisible,
            ChartControllerKeys.DiffRatio => chartState.IsDiffRatioVisible,
            ChartControllerKeys.Distribution => chartState.IsDistributionVisible,
            ChartControllerKeys.WeeklyTrend => chartState.IsWeeklyTrendVisible,
            ChartControllerKeys.Transform => chartState.IsTransformPanelVisible,
            ChartControllerKeys.BarPie => chartState.IsBarPieVisible,
            _ => false
        };
    }

    private static bool HasSecondaryData(ChartDataContext context)
    {
        return context.Data2 != null && context.Data2.Any();
    }


    private static void ClearExtendedCharts(ChartState chartState, Actions actions)
    {
        if (chartState.IsNormalizedVisible)
            actions.ClearChart(ChartControllerKeys.Normalized);

        if (chartState.IsDiffRatioVisible)
            actions.ClearChart(ChartControllerKeys.DiffRatio);

        if (chartState.IsDistributionVisible)
            actions.ClearChart(ChartControllerKeys.Distribution);

        if (chartState.IsWeeklyTrendVisible)
            actions.ClearChart(ChartControllerKeys.WeeklyTrend);

        if (chartState.IsTransformPanelVisible)
            actions.ClearChart(ChartControllerKeys.Transform);

        if (chartState.IsBarPieVisible)
            actions.ClearChart(ChartControllerKeys.BarPie);
    }
}
