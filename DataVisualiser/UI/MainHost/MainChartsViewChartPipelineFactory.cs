using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Models;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Reachability;

namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsViewChartPipelineFactory
{
    public MainChartsViewChartPipelineFactoryResult Create(MainChartsViewChartPipelineFactoryContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var computationEngine = new ChartComputationEngine();
        var renderEngine = new ChartRenderEngine();
        var chartUpdateCoordinator = new ChartUpdateCoordinator(
            computationEngine,
            renderEngine,
            context.TooltipManager,
            context.ChartTimestamps);
        chartUpdateCoordinator.SeriesMode = ChartSeriesMode.RawAndSmoothed;

        var distributionPolarRenderingService = new DistributionPolarRenderingService();
        var weekdayTrendChartUpdateCoordinator = new WeekdayTrendChartUpdateCoordinator(
            new WeekdayTrendRenderingService(),
            context.ChartTimestamps);

        var weeklyDistributionService = CreateWeeklyDistributionService(context.ChartTimestamps);
        var hourlyDistributionService = CreateHourlyDistributionService(context.ChartTimestamps);
        var strategyCutOverService = new StrategyCutOverService(new DataPreparationService(), StrategyReachabilityStoreProbe.Default);
        var chartRenderingOrchestrator = new ChartRenderingOrchestrator(
            chartUpdateCoordinator,
            weeklyDistributionService,
            hourlyDistributionService,
            strategyCutOverService,
            new MetricSelectionService(context.ConnectionString),
            context.ConnectionString);

        return new MainChartsViewChartPipelineFactoryResult(
            computationEngine,
            renderEngine,
            chartUpdateCoordinator,
            weeklyDistributionService,
            hourlyDistributionService,
            distributionPolarRenderingService,
            strategyCutOverService,
            chartRenderingOrchestrator,
            weekdayTrendChartUpdateCoordinator);
    }

    private static IDistributionService CreateWeeklyDistributionService(Dictionary<LiveCharts.Wpf.CartesianChart, List<DateTime>> chartTimestamps)
    {
        var dataPreparationService = new DataPreparationService();
        var strategyCutOverService = new StrategyCutOverService(dataPreparationService, StrategyReachabilityStoreProbe.Default);
        return new WeeklyDistributionService(chartTimestamps, strategyCutOverService);
    }

    private static IDistributionService CreateHourlyDistributionService(Dictionary<LiveCharts.Wpf.CartesianChart, List<DateTime>> chartTimestamps)
    {
        var dataPreparationService = new DataPreparationService();
        var strategyCutOverService = new StrategyCutOverService(dataPreparationService, StrategyReachabilityStoreProbe.Default);
        return new HourlyDistributionService(chartTimestamps, strategyCutOverService);
    }
}
