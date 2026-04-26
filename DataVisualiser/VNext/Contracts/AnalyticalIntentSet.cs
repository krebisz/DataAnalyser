namespace DataVisualiser.VNext.Contracts;

public sealed record AnalyticalIntentSet
{
    public AnalyticalIntentSet(
        MetricSelectionRequest selection,
        IReadOnlyList<AnalyticalIntent> intents)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(intents);

        if (intents.Count == 0)
            throw new ArgumentException("At least one analytical intent is required.", nameof(intents));

        foreach (var intent in intents)
        {
            if (!string.Equals(selection.Signature, intent.Selection.Signature, StringComparison.Ordinal))
                throw new ArgumentException("All analytical intents must share the intent-set selection.", nameof(intents));
        }

        Selection = selection;
        Intents = intents.ToArray();
    }

    public MetricSelectionRequest Selection { get; }
    public IReadOnlyList<AnalyticalIntent> Intents { get; }
    public IReadOnlyList<ChartProgramKind> ProgramKinds => Intents.Select(intent => intent.ProgramRequest.Kind).ToArray();

    public string Signature =>
        $"{Selection.Signature}::{string.Join("|", Intents.Select(intent => $"{intent.ProgramRequest.Kind}:{intent.Signature}"))}";

    public static AnalyticalIntentSet FromIntents(IReadOnlyList<AnalyticalIntent> intents)
    {
        ArgumentNullException.ThrowIfNull(intents);
        if (intents.Count == 0)
            throw new ArgumentException("At least one analytical intent is required.", nameof(intents));

        return new AnalyticalIntentSet(intents[0].Selection, intents);
    }
}
