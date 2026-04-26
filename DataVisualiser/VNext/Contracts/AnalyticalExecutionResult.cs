namespace DataVisualiser.VNext.Contracts;

public sealed record AnalyticalExecutionResult
{
    public AnalyticalExecutionResult(
        AnalyticalIntent intent,
        MetricLoadSnapshot snapshot,
        ChartProgram program)
    {
        ArgumentNullException.ThrowIfNull(intent);
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(program);

        if (!string.Equals(intent.Selection.Signature, snapshot.Signature, StringComparison.Ordinal))
            throw new ArgumentException("Snapshot signature must match the analytical intent selection.", nameof(snapshot));
        if (!string.Equals(snapshot.Signature, program.SourceSignature, StringComparison.Ordinal))
            throw new ArgumentException("Program source signature must match the loaded snapshot.", nameof(program));
        if (program.Kind != intent.ProgramRequest.Kind)
            throw new ArgumentException("Program kind must match the analytical intent request.", nameof(program));

        Intent = intent;
        Snapshot = snapshot;
        Program = program;
    }

    public AnalyticalIntent Intent { get; }
    public MetricLoadSnapshot Snapshot { get; }
    public ChartProgram Program { get; }
    public ProvenanceDescriptor Provenance => Intent.Provenance;

    public string Signature => $"{Intent.Signature}::{Snapshot.Signature}::{Program.Kind}:{Program.SourceSignature}";
}

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
