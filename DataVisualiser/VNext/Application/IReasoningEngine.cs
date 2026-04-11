using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Application;

public interface IReasoningEngine
{
    Task<MetricLoadSnapshot> LoadAsync(MetricSelectionRequest request, CancellationToken cancellationToken = default);
    ChartProgram BuildMainProgram(MetricLoadSnapshot snapshot, ChartDisplayMode displayMode = ChartDisplayMode.Regular);
    ChartProgram BuildNormalizedProgram(MetricLoadSnapshot snapshot);
    ChartProgram BuildDifferenceProgram(MetricLoadSnapshot snapshot);
    ChartProgram BuildRatioProgram(MetricLoadSnapshot snapshot);
}
