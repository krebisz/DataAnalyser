using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed record TransformSelectionResolution(
    MetricSeriesSelection? PrimarySelection,
    MetricSeriesSelection? SecondarySelection,
    bool HasAvailableSecondaryInput);

internal sealed record TransformResolutionResult(
    TransformSelectionResolution Selection,
    IReadOnlyList<MetricData>? PrimaryData,
    IReadOnlyList<MetricData>? SecondaryData,
    ChartDataContext Context);

internal sealed record TransformExecutionResult(
    List<MetricData> DataList,
    List<double> Results,
    string OperationTag,
    int OperationArity,
    List<IReadOnlyList<MetricData>> Metrics,
    string? OverrideLabel);
