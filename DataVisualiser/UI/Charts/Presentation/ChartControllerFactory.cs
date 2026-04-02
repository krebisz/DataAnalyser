using DataVisualiser.Core.Services;
using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Rendering.WeekdayTrend;
using DataVisualiser.Core.Rendering.BarPie;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Presentation.Rendering;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.Core.Rendering.Distribution;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using System.Windows.Controls;
namespace DataVisualiser.UI.Charts.Presentation;

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

        BindControllerEvents(
            context,
            mainAdapter,
            normalizedAdapter,
            diffRatioAdapter,
            distributionAdapter,
            weekdayTrendAdapter,
            transformAdapter,
            barPieAdapter);

        return new ChartControllerFactoryResult(mainAdapter, normalizedAdapter, diffRatioAdapter, distributionAdapter, weekdayTrendAdapter, transformAdapter, barPieAdapter, registry);
    }

    private static void BindControllerEvents(
        ChartControllerFactoryContext context,
        MainChartControllerAdapter mainAdapter,
        NormalizedChartControllerAdapter normalizedAdapter,
        DiffRatioChartControllerAdapter diffRatioAdapter,
        DistributionChartControllerAdapter distributionAdapter,
        WeekdayTrendChartControllerAdapter weekdayTrendAdapter,
        TransformDataPanelControllerAdapter transformAdapter,
        BarPieChartControllerAdapter barPieAdapter)
    {
        context.MainChartController.ToggleRequested += mainAdapter.OnToggleRequested;
        context.MainChartController.DisplayModeChanged += mainAdapter.OnDisplayModeChanged;
        context.MainChartController.OverlaySubtypeChanged += mainAdapter.OnOverlaySubtypeChanged;

        context.WeekdayTrendChartController.ToggleRequested += weekdayTrendAdapter.OnToggleRequested;
        context.WeekdayTrendChartController.ChartTypeToggleRequested += weekdayTrendAdapter.OnChartTypeToggleRequested;
        context.WeekdayTrendChartController.DayToggled += weekdayTrendAdapter.OnDayToggled;
        context.WeekdayTrendChartController.AverageToggled += weekdayTrendAdapter.OnAverageToggled;
        context.WeekdayTrendChartController.AverageWindowChanged += weekdayTrendAdapter.OnAverageWindowChanged;
        context.WeekdayTrendChartController.SubtypeChanged += weekdayTrendAdapter.OnSubtypeChanged;

        context.DiffRatioChartController.ToggleRequested += diffRatioAdapter.OnToggleRequested;
        context.DiffRatioChartController.OperationToggleRequested += diffRatioAdapter.OnOperationToggleRequested;
        context.DiffRatioChartController.PrimarySubtypeChanged += diffRatioAdapter.OnPrimarySubtypeChanged;
        context.DiffRatioChartController.SecondarySubtypeChanged += diffRatioAdapter.OnSecondarySubtypeChanged;

        context.BarPieChartController.ToggleRequested += barPieAdapter.OnToggleRequested;
        context.BarPieChartController.DisplayModeChanged += barPieAdapter.OnDisplayModeChanged;
        context.BarPieChartController.BucketCountChanged += barPieAdapter.OnBucketCountChanged;

        context.NormalizedChartController.ToggleRequested += normalizedAdapter.OnToggleRequested;
        context.NormalizedChartController.NormalizationModeChanged += normalizedAdapter.OnNormalizationModeChanged;
        context.NormalizedChartController.PrimarySubtypeChanged += normalizedAdapter.OnPrimarySubtypeChanged;
        context.NormalizedChartController.SecondarySubtypeChanged += normalizedAdapter.OnSecondarySubtypeChanged;

        context.DistributionChartController.ToggleRequested += distributionAdapter.OnToggleRequested;
        context.DistributionChartController.ChartTypeToggleRequested += distributionAdapter.OnChartTypeToggleRequested;
        context.DistributionChartController.ModeChanged += distributionAdapter.OnModeChanged;
        context.DistributionChartController.SubtypeChanged += distributionAdapter.OnSubtypeChanged;
        context.DistributionChartController.DisplayModeChanged += distributionAdapter.OnDisplayModeChanged;
        context.DistributionChartController.IntervalCountChanged += distributionAdapter.OnIntervalCountChanged;

        context.TransformDataPanelController.ToggleRequested += transformAdapter.OnToggleRequested;
        context.TransformDataPanelController.OperationChanged += transformAdapter.OnOperationChanged;
        context.TransformDataPanelController.PrimarySubtypeChanged += transformAdapter.OnPrimarySubtypeChanged;
        context.TransformDataPanelController.SecondarySubtypeChanged += transformAdapter.OnSecondarySubtypeChanged;
        context.TransformDataPanelController.ComputeRequested += transformAdapter.OnComputeRequested;
    }
}

public sealed record ChartControllerFactoryContext(
    IMainChartController MainChartController,
    INormalizedChartController NormalizedChartController,
    IDiffRatioChartController DiffRatioChartController,
    IDistributionChartController DistributionChartController,
    IWeekdayTrendChartController WeekdayTrendChartController,
    ITransformDataPanelController TransformDataPanelController,
    IBarPieChartController BarPieChartController,
    MainWindowViewModel ViewModel,
    Func<bool> IsInitializing,
    Func<IDisposable> BeginUiBusyScope,
    MetricSelectionService MetricSelectionService,
    Func<ChartRenderingOrchestrator?> GetChartRenderingOrchestrator,
    ChartUpdateCoordinator ChartUpdateCoordinator,
    Func<IStrategyCutOverService?> GetStrategyCutOverService,
    WeekdayTrendChartUpdateCoordinator WeekdayTrendChartUpdateCoordinator,
    IDistributionService WeeklyDistributionService,
    IDistributionService HourlyDistributionService,
    DistributionPolarRenderingService DistributionPolarRenderingService,
    Func<ToolTip?> GetPolarTooltip,
    Func<ChartTooltipManager?> GetTooltipManager,
    IChartRendererResolver ChartRendererResolver,
    IChartSurfaceFactory ChartSurfaceFactory,
    ISyncfusionSunburstChartController? SyncfusionSunburstChartController = null);

public sealed record ChartControllerFactoryResult(
    MainChartControllerAdapter Main,
    NormalizedChartControllerAdapter Normalized,
    DiffRatioChartControllerAdapter DiffRatio,
    DistributionChartControllerAdapter Distribution,
    WeekdayTrendChartControllerAdapter WeekdayTrend,
    TransformDataPanelControllerAdapter Transform,
    BarPieChartControllerAdapter BarPie,
    IChartControllerRegistry Registry);

public sealed record SyncfusionChartControllerFactoryResult(
    SyncfusionSunburstChartControllerAdapter Sunburst,
    IChartControllerRegistry Registry);
