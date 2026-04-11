using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.UI.State;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Transform;

public enum TransformBackendKey
{
    LiveChartsWpfResultCartesian = 0
}

public enum TransformRenderingRoute
{
    ResultCartesian = 0
}

public enum TransformRenderingQualification
{
    Qualified = 0,
    TacticalFallback = 1,
    UnqualifiedDebt = 2
}

public sealed record TransformChartRenderRequest(
    TransformRenderingRoute Route,
    ChartDataContext Context,
    IChartComputationStrategy Strategy,
    string PrimaryLabel,
    string? OperationType,
    bool IsOperationChart,
    double MinHeight = 400.0);

public sealed record TransformChartRenderHost(
    CartesianChart Chart,
    ChartState ChartState,
    Action? ResetAuxiliaryVisuals = null);
