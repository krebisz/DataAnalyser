using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.State;

public sealed record SelectionState(
    string? MetricType,
    IReadOnlyList<MetricSeriesRequest> Series,
    DateTime? From,
    DateTime? To,
    string? ResolutionTableName)
{
    public static SelectionState Empty { get; } = new(null, Array.Empty<MetricSeriesRequest>(), null, null, null);

    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(MetricType) &&
        !string.IsNullOrWhiteSpace(ResolutionTableName) &&
        From.HasValue &&
        To.HasValue &&
        Series.Count > 0;

    public MetricSelectionRequest ToRequest()
    {
        if (!IsComplete)
            throw new InvalidOperationException("Selection is incomplete.");

        return new MetricSelectionRequest(MetricType!, Series, From!.Value, To!.Value, ResolutionTableName!);
    }
}
