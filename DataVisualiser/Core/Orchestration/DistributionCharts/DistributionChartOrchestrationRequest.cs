using DataFileReader.Canonical;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.Core.Orchestration.DistributionCharts;

public sealed record DistributionChartOrchestrationRequest(
    ChartDataContext Context,
    ChartState ChartState,
    DistributionMode Mode);

public sealed record DistributionChartPreparedData(
    IDistributionService DistributionService,
    IReadOnlyList<MetricData> Data,
    string DisplayName,
    DateTime From,
    DateTime To,
    DistributionModeSettings Settings,
    ICanonicalMetricSeries? CmsSeries);
