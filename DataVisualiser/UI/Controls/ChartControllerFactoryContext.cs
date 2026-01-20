using System;
using System.Windows.Controls;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.UI.Rendering.LiveCharts;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Controls;

public sealed record ChartControllerFactoryContext(
    MainChartController MainChartController,
    NormalizedChartController NormalizedChartController,
    DiffRatioChartController DiffRatioChartController,
    DistributionChartController DistributionChartController,
    WeekdayTrendChartController WeekdayTrendChartController,
    TransformDataPanelController TransformDataPanelController,
    BarPieChartController BarPieChartController,
    MainWindowViewModel ViewModel,
    Func<bool> IsInitializing,
    Func<IDisposable> BeginUiBusyScope,
    MetricSelectionService MetricSelectionService,
    Func<ChartRenderingOrchestrator?> GetChartRenderingOrchestrator,
    ChartUpdateCoordinator ChartUpdateCoordinator,
    Func<IStrategyCutOverService?> GetStrategyCutOverService,
    WeekdayTrendChartUpdateCoordinator WeekdayTrendChartUpdateCoordinator,
    WeeklyDistributionService WeeklyDistributionService,
    HourlyDistributionService HourlyDistributionService,
    DistributionPolarRenderingService DistributionPolarRenderingService,
    Func<ToolTip?> GetPolarTooltip,
    Func<ChartTooltipManager?> GetTooltipManager);
