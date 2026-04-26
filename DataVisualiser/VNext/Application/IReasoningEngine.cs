using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Application;

public interface IReasoningEngine
{
    Task<MetricLoadSnapshot> LoadAsync(MetricSelectionRequest request, CancellationToken cancellationToken = default);
    Task<AnalyticalExecutionResult> ExecuteAsync(AnalyticalIntent intent, CancellationToken cancellationToken = default);
    ChartProgram BuildProgram(MetricLoadSnapshot snapshot, ChartProgramRequest request);
    ChartProgram BuildProgram(MetricLoadSnapshot snapshot, AnalyticalIntent intent);
    ChartProgram BuildMainProgram(MetricLoadSnapshot snapshot, ChartDisplayMode displayMode = ChartDisplayMode.Regular);
    ChartProgram BuildNormalizedProgram(MetricLoadSnapshot snapshot);
    ChartProgram BuildDifferenceProgram(MetricLoadSnapshot snapshot);
    ChartProgram BuildRatioProgram(MetricLoadSnapshot snapshot);
}
