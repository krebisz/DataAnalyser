using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Core.Rendering.Transform;

public sealed record TransformChartRenderRequest(
    TransformRenderingRoute Route,
    ChartDataContext Context,
    IChartComputationStrategy Strategy,
    string PrimaryLabel,
    string? OperationType,
    bool IsOperationChart,
    double MinHeight = 400.0);
