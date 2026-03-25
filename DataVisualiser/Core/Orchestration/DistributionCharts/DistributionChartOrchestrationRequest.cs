using DataVisualiser.UI.State;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Orchestration.DistributionCharts;

public sealed record DistributionChartOrchestrationRequest(
    ChartDataContext Context,
    ChartState ChartState,
    DistributionMode Mode);
