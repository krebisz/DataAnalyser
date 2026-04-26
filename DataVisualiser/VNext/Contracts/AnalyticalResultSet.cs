namespace DataVisualiser.VNext.Contracts;

public sealed record AnalyticalResultSet
{
    public AnalyticalResultSet(
        MetricSelectionRequest selection,
        IReadOnlyList<AnalyticalExecutionResult> results)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(results);

        if (results.Count == 0)
            throw new ArgumentException("At least one analytical execution result is required.", nameof(results));

        foreach (var result in results)
        {
            if (!string.Equals(selection.Signature, result.Intent.Selection.Signature, StringComparison.Ordinal))
                throw new ArgumentException("All analytical execution results must share the result-set selection.", nameof(results));
        }

        Selection = selection;
        Results = results.ToArray();
    }

    public MetricSelectionRequest Selection { get; }
    public IReadOnlyList<AnalyticalExecutionResult> Results { get; }
    public IReadOnlyList<ChartProgramKind> ProgramKinds => Results.Select(result => result.Program.Kind).ToArray();

    public string Signature =>
        $"{Selection.Signature}::{string.Join("|", Results.Select(result => $"{result.Program.Kind}:{result.Program.SourceSignature}"))}";
}
