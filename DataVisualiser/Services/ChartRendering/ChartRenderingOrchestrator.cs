using DataVisualiser.Charts;
using DataVisualiser.Charts.Computation;
using DataVisualiser.Charts.Strategies;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;
using DataVisualiser.State;
using LiveCharts.Wpf;

namespace DataVisualiser.Services.ChartRendering
{
    /// <summary>
    /// Orchestrates chart rendering operations, extracting complex rendering logic
    /// from MainWindow to improve maintainability and testability.
    /// Handles multi-chart rendering, visibility management, and chart-specific rendering strategies.
    /// </summary>
    public sealed class ChartRenderingOrchestrator
    {
        private readonly ChartUpdateCoordinator _chartUpdateCoordinator;
        private readonly WeeklyDistributionService _weeklyDistributionService;
        private readonly IStrategyCutOverService _strategyCutOverService;

        public ChartRenderingOrchestrator(
            ChartUpdateCoordinator chartUpdateCoordinator,
            WeeklyDistributionService weeklyDistributionService,
            IStrategyCutOverService strategyCutOverService)
        {
            _chartUpdateCoordinator = chartUpdateCoordinator ?? throw new ArgumentNullException(nameof(chartUpdateCoordinator));
            _weeklyDistributionService = weeklyDistributionService ?? throw new ArgumentNullException(nameof(weeklyDistributionService));
            _strategyCutOverService = strategyCutOverService ?? throw new ArgumentNullException(nameof(strategyCutOverService));
        }

        /// <summary>
        /// Renders all charts based on the provided context and visibility state.
        /// </summary>
        public async Task RenderChartsFromContext(
            ChartDataContext ctx,
            ChartState chartState,
            CartesianChart chartMain,
            CartesianChart chartNorm,
            CartesianChart chartDiff,
            CartesianChart chartRatio,
            CartesianChart chartWeekly)
        {
            if (!ShouldRenderCharts(ctx))
                return;

            var hasSecondaryData = HasSecondaryData(ctx);
            var metricType = ctx.MetricType;
            var primarySubtype = ctx.PrimarySubtype;
            var secondarySubtype = ctx.SecondarySubtype;

            // Render primary chart if visible
            if (chartState.IsMainVisible)
            {
                await RenderPrimaryChart(ctx, chartMain);
            }

            // Render secondary charts if visible and data available
            if (hasSecondaryData)
            {
                if (chartState.IsNormalizedVisible)
                {
                    await RenderNormalized(ctx, chartNorm, metricType, primarySubtype, secondarySubtype, chartState.SelectedNormalizationMode);
                }

                if (chartState.IsDifferenceVisible)
                {
                    await RenderDifference(ctx, chartDiff, metricType, primarySubtype, secondarySubtype);
                }

                if (chartState.IsRatioVisible)
                {
                    await RenderRatio(ctx, chartRatio, metricType, primarySubtype, secondarySubtype);
                }
            }
            else
            {
                // Clear charts that require secondary data when no secondary data exists
                Charts.Helpers.ChartHelper.ClearChart(chartNorm, chartState.ChartTimestamps);
                Charts.Helpers.ChartHelper.ClearChart(chartDiff, chartState.ChartTimestamps);
                Charts.Helpers.ChartHelper.ClearChart(chartRatio, chartState.ChartTimestamps);
            }

            // Render charts that don't require secondary data
            if (chartState.IsWeeklyVisible)
            {
                await RenderWeeklyDistribution(ctx, chartWeekly, chartState);
            }

            if (chartState.IsWeeklyTrendVisible)
            {
                RenderWeeklyTrend(ctx);
            }
        }

        /// <summary>
        /// Renders a single chart by name.
        /// </summary>
        public async Task RenderSingleChart(
            string chartName,
            ChartDataContext ctx,
            ChartState chartState,
            CartesianChart chartMain,
            CartesianChart chartNorm,
            CartesianChart chartDiff,
            CartesianChart chartRatio,
            CartesianChart chartWeekly)
        {
            var hasSecondaryData = HasSecondaryData(ctx);
            var metricType = ctx.MetricType;
            var primarySubtype = ctx.PrimarySubtype;
            var secondarySubtype = ctx.SecondarySubtype;

            switch (chartName)
            {
                case "Main":
                    if (chartState.IsMainVisible)
                    {
                        await RenderPrimaryChart(ctx, chartMain);
                    }
                    break;

                case "Norm":
                    if (chartState.IsNormalizedVisible && hasSecondaryData)
                    {
                        await RenderNormalized(ctx, chartNorm, metricType, primarySubtype, secondarySubtype, chartState.SelectedNormalizationMode);
                    }
                    break;

                case "Diff":
                    if (chartState.IsDifferenceVisible && hasSecondaryData)
                    {
                        await RenderDifference(ctx, chartDiff, metricType, primarySubtype, secondarySubtype);
                    }
                    break;

                case "Ratio":
                    if (chartState.IsRatioVisible && hasSecondaryData)
                    {
                        await RenderRatio(ctx, chartRatio, metricType, primarySubtype, secondarySubtype);
                    }
                    break;

                case "Weekly":
                    if (chartState.IsWeeklyVisible)
                    {
                        await RenderWeeklyDistribution(ctx, chartWeekly, chartState);
                    }
                    break;

                case "WeeklyTrend":
                    if (chartState.IsWeeklyTrendVisible)
                    {
                        RenderWeeklyTrend(ctx);
                    }
                    break;
            }
        }

        /// <summary>
        /// Renders the primary (main) chart. This delegates to MainWindow's RenderMainChart
        /// which handles the complex multi-series logic.
        /// </summary>
        public async Task RenderPrimaryChart(ChartDataContext ctx, CartesianChart chartMain)
        {
            // This method is a placeholder - actual rendering is handled by MainWindow.RenderMainChart
            // which has complex logic for building series lists and loading additional subtypes
            await Task.CompletedTask;
        }

        private async Task RenderNormalized(
            ChartDataContext ctx,
            CartesianChart chartNorm,
            string? metricType,
            string? primarySubtype,
            string? secondarySubtype,
            NormalizationMode normalizationMode)
        {
            var strategy = new NormalizedStrategy(
                ctx.Data1!,
                ctx.Data2!,
                ctx.DisplayName1,
                ctx.DisplayName2,
                ctx.From,
                ctx.To,
                normalizationMode);

            await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(
                chartNorm,
                strategy,
                $"{ctx.DisplayName1} ~ {ctx.DisplayName2}",
                minHeight: 400,
                metricType: metricType,
                primarySubtype: primarySubtype,
                secondarySubtype: secondarySubtype,
                operationType: "~",
                isOperationChart: true);
        }

        private async Task RenderDifference(
            ChartDataContext ctx,
            CartesianChart chartDiff,
            string? metricType,
            string? primarySubtype,
            string? secondarySubtype)
        {
            var strategy = new DifferenceStrategy(
                ctx.Data1!,
                ctx.Data2!,
                ctx.DisplayName1,
                ctx.DisplayName2,
                ctx.From,
                ctx.To);

            await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(
                chartDiff,
                strategy,
                $"{ctx.DisplayName1} - {ctx.DisplayName2}",
                minHeight: 400,
                metricType: metricType,
                primarySubtype: primarySubtype,
                secondarySubtype: secondarySubtype,
                operationType: "-",
                isOperationChart: true);
        }

        private async Task RenderRatio(
            ChartDataContext ctx,
            CartesianChart chartRatio,
            string? metricType,
            string? primarySubtype,
            string? secondarySubtype)
        {
            var strategy = new RatioStrategy(
                ctx.Data1!,
                ctx.Data2!,
                ctx.DisplayName1,
                ctx.DisplayName2,
                ctx.From,
                ctx.To);

            await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(
                chartRatio,
                strategy,
                $"{ctx.DisplayName1} / {ctx.DisplayName2}",
                minHeight: 400,
                metricType: metricType,
                primarySubtype: primarySubtype,
                secondarySubtype: secondarySubtype,
                operationType: "/",
                isOperationChart: true);
        }

        private async Task RenderWeeklyDistribution(
            ChartDataContext ctx,
            CartesianChart chartWeekly,
            ChartState chartState)
        {
            await _weeklyDistributionService.UpdateWeeklyDistributionChartAsync(
                chartWeekly,
                ctx.Data1!,
                ctx.DisplayName1,
                ctx.From,
                ctx.To,
                minHeight: 400,
                useFrequencyShading: chartState.UseFrequencyShading,
                intervalCount: chartState.WeeklyIntervalCount);
        }

        private void RenderWeeklyTrend(ChartDataContext ctx)
        {
            // Implementation extracted from MainWindow
            // TODO: Implement weekly trend rendering
        }

        private static bool ShouldRenderCharts(ChartDataContext? ctx)
        {
            return ctx != null && ctx.Data1 != null && ctx.Data1.Any();
        }

        private static bool HasSecondaryData(ChartDataContext ctx)
        {
            return ctx.Data2 != null && ctx.Data2.Any();
        }
    }
}
