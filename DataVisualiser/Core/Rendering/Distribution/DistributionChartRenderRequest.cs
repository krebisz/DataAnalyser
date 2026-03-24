using DataFileReader.Canonical;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.Core.Rendering.Distribution;

public sealed record DistributionChartRenderRequest(
    DistributionRenderingRoute Route,
    DistributionMode Mode,
    DistributionModeSettings Settings,
    IReadOnlyList<MetricData> Data,
    string DisplayName,
    DateTime From,
    DateTime To,
    ICanonicalMetricSeries? CmsSeries,
    ChartDataContext RenderingContext,
    ChartState ChartState);
