using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Core.Computation.Results;

public static class ComputedSeriesResultBuilder
{
    public static ComputedSeriesResult Build(
        string id,
        string label,
        IReadOnlyList<MetricData> dataList,
        IReadOnlyList<double> rawValues,
        IReadOnlyList<double> smoothedValues,
        IReadOnlyList<IReadOnlyList<MetricData>> sourceMetrics,
        string? operationSignature = null)
    {
        ArgumentNullException.ThrowIfNull(dataList);
        ArgumentNullException.ThrowIfNull(rawValues);
        ArgumentNullException.ThrowIfNull(smoothedValues);
        ArgumentNullException.ThrowIfNull(sourceMetrics);

        return new ComputedSeriesResult(
            string.IsNullOrWhiteSpace(id) ? "computed" : id,
            string.IsNullOrWhiteSpace(label) ? "Computed Series" : label,
            dataList.Select(point => point.NormalizedTimestamp).ToArray(),
            rawValues,
            smoothedValues,
            sourceMetrics.Select((_, index) => $"input-{index}").ToArray(),
            operationSignature ?? id);
    }
}
