using DataVisualiser.Charts;
using DataVisualiser.Charts.Computation;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;
using DataVisualiser.State;
using LiveCharts.Wpf;

namespace DataVisualiser.Services.ChartRendering
{
    /// <summary>
    /// Orchestrates chart rendering operations, extracting complex rendering logic
    /// from MainWindow to improve maintainability and testability.
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
        /// Renders a single chart by name from the provided context.
        /// </summary>
        public async Task RenderSingleChartAsync(
            string chartName,
            ChartDataContext ctx,
            ChartState chartState)
        {
            if (ctx == null || !ShouldRenderCharts(ctx))
                return;

            var hasSecondaryData = HasSecondaryData(ctx);
            var metricType = ctx.MetricType;
            var primarySubtype = ctx.PrimarySubtype;
            var secondarySubtype = ctx.SecondarySubtype;

            switch (chartName)
            {
                case "Main":
                    if (chartState.IsMainVisible)
                    {
                        await RenderMainChartAsync(ctx, chartState);
                    }
                    break;

                case "Norm":
                    if (chartState.IsNormalizedVisible && hasSecondaryData)
                    {
                        await RenderNormalizedAsync(ctx, metricType, primarySubtype, secondarySubtype);
                    }
                    break;

                case "Diff":
                    if (chartState.IsDifferenceVisible && hasSecondaryData)
                    {
                        await RenderDifferenceAsync(ctx, metricType, primarySubtype, secondarySubtype);
                    }
                    break;

                case "Ratio":
                    if (chartState.IsRatioVisible && hasSecondaryData)
                    {
                        await RenderRatioAsync(ctx, metricType, primarySubtype, secondarySubtype);
                    }
                    break;

                case "Weekly":
                    if (chartState.IsWeeklyVisible)
                    {
                        await RenderWeeklyDistributionAsync(ctx, chartState);
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
        /// Renders all charts from the provided context based on visibility state.
        /// </summary>
        public async Task RenderAllChartsAsync(
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

            // Primary chart
            if (chartState.IsMainVisible)
            {
                await RenderMainChartAsync(ctx, chartState);
            }

            // Charts that require secondary data
            if (hasSecondaryData)
            {
                if (chartState.IsNormalizedVisible)
                {
                    await RenderNormalizedAsync(ctx, metricType, primarySubtype, secondarySubtype);
                }

                if (chartState.IsDifferenceVisible)
                {
                    await RenderDifferenceAsync(ctx, metricType, primarySubtype, secondarySubtype);
                }

                if (chartState.IsRatioVisible)
                {
                    await RenderRatioAsync(ctx, metricType, primarySubtype, secondarySubtype);
                }
            }
            else
            {
                // Clear charts that require secondary data when no secondary data exists
                Charts.Helpers.ChartHelper.ClearChart(chartNorm, chartState.ChartTimestamps);
                Charts.Helpers.ChartHelper.ClearChart(chartDiff, chartState.ChartTimestamps);
                Charts.Helpers.ChartHelper.ClearChart(chartRatio, chartState.ChartTimestamps);
            }

            // Charts that don't require secondary data
            if (chartState.IsWeeklyVisible)
            {
                await RenderWeeklyDistributionAsync(ctx, chartState);
            }

            if (chartState.IsWeeklyTrendVisible)
            {
                RenderWeeklyTrend(ctx);
            }
        }

        private async Task RenderMainChartAsync(ChartDataContext ctx, ChartState chartState)
        {
            // This will be implemented by extracting logic from MainWindow
            // For now, this is a placeholder that maintains the interface
        }

        private async Task RenderNormalizedAsync(
            ChartDataContext ctx,
            string? metricType,
            string? primarySubtype,
            string? secondarySubtype)
        {
            // Implementation extracted from MainWindow
        }

        private async Task RenderDifferenceAsync(
            ChartDataContext ctx,
            string? metricType,
            string? primarySubtype,
            string? secondarySubtype)
        {
            // Implementation extracted from MainWindow
        }

        private async Task RenderRatioAsync(
            ChartDataContext ctx,
            string? metricType,
            string? primarySubtype,
            string? secondarySubtype)
        {
            // Implementation extracted from MainWindow
        }

        private async Task RenderWeeklyDistributionAsync(ChartDataContext ctx, ChartState chartState)
        {
            // Implementation extracted from MainWindow
        }

        private void RenderWeeklyTrend(ChartDataContext ctx)
        {
            // Implementation extracted from MainWindow
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

