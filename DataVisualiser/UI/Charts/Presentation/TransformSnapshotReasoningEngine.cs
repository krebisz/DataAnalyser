using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TransformSnapshotReasoningEngine(MetricLoadSnapshotGateway gateway) : IReasoningEngine
{
    public Task<MetricLoadSnapshot> LoadAsync(MetricSelectionRequest request, CancellationToken cancellationToken = default) =>
        gateway.LoadAsync(request, cancellationToken);

    public Task<AnalyticalExecutionResult> ExecuteAsync(AnalyticalIntent intent, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<AnalyticalResultSet> ExecuteAsync(AnalyticalIntentSet intentSet, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public ChartProgram BuildProgram(MetricLoadSnapshot snapshot, ChartProgramRequest request) =>
        throw new NotSupportedException();

    public ChartProgram BuildProgram(MetricLoadSnapshot snapshot, AnalyticalIntent intent) =>
        throw new NotSupportedException();

    public ChartProgram BuildMainProgram(MetricLoadSnapshot snapshot, ChartDisplayMode displayMode = ChartDisplayMode.Regular) =>
        throw new NotSupportedException();

    public ChartProgram BuildNormalizedProgram(MetricLoadSnapshot snapshot) =>
        throw new NotSupportedException();

    public ChartProgram BuildDifferenceProgram(MetricLoadSnapshot snapshot) =>
        throw new NotSupportedException();

    public ChartProgram BuildRatioProgram(MetricLoadSnapshot snapshot) =>
        throw new NotSupportedException();
}
