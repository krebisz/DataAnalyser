using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Rendering;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Reachability;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.MainHost.Coordination;

public sealed class MainChartsViewChartPipelineFactory
{
    public sealed record Context(
        Dictionary<CartesianChart, List<DateTime>> ChartTimestamps,
        ChartTooltipManager TooltipManager,
        string ConnectionString);

    public sealed record Result(
        ChartComputationEngine ComputationEngine,
        ChartRenderEngine RenderEngine,
        ChartUpdateCoordinator ChartUpdateCoordinator,
        IDistributionService WeeklyDistributionService,
        IDistributionService HourlyDistributionService,
        DistributionPolarRenderingService DistributionPolarRenderingService,
        IStrategyCutOverService StrategyCutOverService,
        ChartRenderingOrchestrator ChartRenderingOrchestrator,
        WeekdayTrendChartUpdateCoordinator WeekdayTrendChartUpdateCoordinator);

    public Result Create(Context context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        IUserNotificationService notificationService = MessageBoxUserNotificationService.Instance;
        var computationEngine = new ChartComputationEngine();
        var renderEngine = new ChartRenderEngine();
        var chartUpdateCoordinator = new ChartUpdateCoordinator(
            computationEngine,
            renderEngine,
            context.TooltipManager,
            context.ChartTimestamps,
            notificationService);
        chartUpdateCoordinator.SeriesMode = ChartSeriesMode.RawAndSmoothed;

        var distributionPolarRenderingService = new DistributionPolarRenderingService();
        var weekdayTrendChartUpdateCoordinator = new WeekdayTrendChartUpdateCoordinator(
            new WeekdayTrendRenderingService(),
            context.ChartTimestamps);

        var weeklyDistributionService = CreateWeeklyDistributionService(context.ChartTimestamps, notificationService);
        var hourlyDistributionService = CreateHourlyDistributionService(context.ChartTimestamps, notificationService);
        var strategyCutOverService = new StrategyCutOverService(new DataPreparationService(), StrategyReachabilityStoreProbe.Default);
        var chartRenderingOrchestrator = new ChartRenderingOrchestrator(
            chartUpdateCoordinator,
            weeklyDistributionService,
            hourlyDistributionService,
            strategyCutOverService,
            new MetricSelectionService(context.ConnectionString),
            notificationService,
            context.ConnectionString);

        return new Result(
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

    private static IDistributionService CreateWeeklyDistributionService(Dictionary<CartesianChart, List<DateTime>> chartTimestamps, IUserNotificationService notificationService)
    {
        var dataPreparationService = new DataPreparationService();
        var strategyCutOverService = new StrategyCutOverService(dataPreparationService, StrategyReachabilityStoreProbe.Default);
        return new WeeklyDistributionService(chartTimestamps, strategyCutOverService, notificationService);
    }

    private static IDistributionService CreateHourlyDistributionService(Dictionary<CartesianChart, List<DateTime>> chartTimestamps, IUserNotificationService notificationService)
    {
        var dataPreparationService = new DataPreparationService();
        var strategyCutOverService = new StrategyCutOverService(dataPreparationService, StrategyReachabilityStoreProbe.Default);
        return new HourlyDistributionService(chartTimestamps, strategyCutOverService, notificationService);
    }
}
