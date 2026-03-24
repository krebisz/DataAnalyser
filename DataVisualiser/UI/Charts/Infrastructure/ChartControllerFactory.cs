using DataVisualiser.Core.Services;
using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Rendering.WeekdayTrend;
using DataVisualiser.Core.Rendering.BarPie;
using DataVisualiser.UI.Charts.Adapters;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.Core.Rendering.Distribution;
namespace DataVisualiser.UI.Charts.Infrastructure;

public sealed class ChartControllerFactory : IChartControllerFactory
{
    public SyncfusionChartControllerFactoryResult CreateSyncfusion(
        ISyncfusionSunburstChartController controller,
        MainWindowViewModel viewModel,
        MetricSelectionService metricSelectionService)
    {
        if (controller == null)
            throw new ArgumentNullException(nameof(controller));
        if (viewModel == null)
            throw new ArgumentNullException(nameof(viewModel));
        if (metricSelectionService == null)
            throw new ArgumentNullException(nameof(metricSelectionService));

        var adapter = new SyncfusionSunburstChartControllerAdapter(controller, viewModel, metricSelectionService);
        var registry = new ChartControllerRegistry();
        registry.Register(adapter);

        return new SyncfusionChartControllerFactoryResult(adapter, registry);
    }

    public ChartControllerFactoryResult Create(ChartControllerFactoryContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var cartesianMetricRenderingContract = new CartesianMetricChartRenderingContract(
            new CartesianMetricChartRenderInvoker(context.GetChartRenderingOrchestrator));

        var mainAdapter = new MainChartControllerAdapter(context.MainChartController, context.ViewModel, context.IsInitializing, context.MetricSelectionService, cartesianMetricRenderingContract);

        var distributionRenderingContract = new DistributionRenderingContract(
            context.GetChartRenderingOrchestrator,
            context.WeeklyDistributionService,
            context.HourlyDistributionService,
            context.DistributionPolarRenderingService);

        var distributionAdapter = new DistributionChartControllerAdapter(
            context.DistributionChartController,
            context.ViewModel,
            context.IsInitializing,
            context.BeginUiBusyScope,
            context.MetricSelectionService,
            distributionRenderingContract,
            context.GetPolarTooltip);

        var weekdayTrendRenderingContract = new WeekdayTrendRenderingContract(context.WeekdayTrendChartUpdateCoordinator);

        var weekdayTrendAdapter = new WeekdayTrendChartControllerAdapter(context.WeekdayTrendChartController, context.ViewModel, context.IsInitializing, context.BeginUiBusyScope, context.MetricSelectionService, context.GetStrategyCutOverService, weekdayTrendRenderingContract);

        var normalizedAdapter = new NormalizedChartControllerAdapter(context.NormalizedChartController, context.ViewModel, context.IsInitializing, context.BeginUiBusyScope, context.MetricSelectionService, cartesianMetricRenderingContract);

        var diffRatioAdapter = new DiffRatioChartControllerAdapter(context.DiffRatioChartController, context.ViewModel, context.IsInitializing, context.BeginUiBusyScope, context.MetricSelectionService, context.GetTooltipManager, cartesianMetricRenderingContract);

        var transformRenderingContract = new TransformRenderingContract(new TransformChartRenderInvoker(context.ChartUpdateCoordinator));

        var transformAdapter = new TransformDataPanelControllerAdapter(context.TransformDataPanelController, context.ViewModel, context.IsInitializing, context.BeginUiBusyScope, context.MetricSelectionService, transformRenderingContract);

        var barPieRenderingContract = new BarPieRenderingContract();
        var barPieAdapter = new BarPieChartControllerAdapter(
            context.BarPieChartController,
            context.ViewModel,
            context.IsInitializing,
            context.MetricSelectionService,
            barPieRenderingContract,
            context.ChartRendererResolver,
            context.ChartSurfaceFactory);

        var registry = new ChartControllerRegistry();
        registry.Register(mainAdapter);
        registry.Register(normalizedAdapter);
        registry.Register(diffRatioAdapter);
        registry.Register(distributionAdapter);
        registry.Register(weekdayTrendAdapter);
        registry.Register(transformAdapter);
        registry.Register(barPieAdapter);

        if (context.SyncfusionSunburstChartController != null)
        {
            var syncfusionAdapter = new SyncfusionSunburstChartControllerAdapter(
                context.SyncfusionSunburstChartController,
                context.ViewModel,
                context.MetricSelectionService);
            registry.Register(syncfusionAdapter);
        }

        return new ChartControllerFactoryResult(mainAdapter, normalizedAdapter, diffRatioAdapter, distributionAdapter, weekdayTrendAdapter, transformAdapter, barPieAdapter, registry);
    }
}
