using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.UI.MainHost;

public sealed record MainChartsViewChartPipelineFactoryResult(
    ChartComputationEngine ComputationEngine,
    ChartRenderEngine RenderEngine,
    ChartUpdateCoordinator ChartUpdateCoordinator,
    IDistributionService WeeklyDistributionService,
    IDistributionService HourlyDistributionService,
    DistributionPolarRenderingService DistributionPolarRenderingService,
    IStrategyCutOverService StrategyCutOverService,
    ChartRenderingOrchestrator ChartRenderingOrchestrator,
    WeekdayTrendChartUpdateCoordinator WeekdayTrendChartUpdateCoordinator);
