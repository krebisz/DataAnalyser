using System.Windows.Controls;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.UI.Rendering;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Controls;

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
    IChartSurfaceFactory ChartSurfaceFactory);
