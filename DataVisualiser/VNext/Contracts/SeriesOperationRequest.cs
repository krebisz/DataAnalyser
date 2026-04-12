namespace DataVisualiser.VNext.Contracts;

public enum SeriesOperationKind
{
    Identity = 0,
    Normalize = 1,
    Sum = 2,
    Difference = 3,
    Ratio = 4
}

public sealed record SeriesOperationRequest
{
    public SeriesOperationRequest(
        SeriesOperationKind kind,
        IReadOnlyList<int> inputIndexes,
        string id,
        string label)
    {
        if (inputIndexes == null || inputIndexes.Count == 0)
            throw new ArgumentException("At least one input index is required.", nameof(inputIndexes));
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Operation id cannot be null or empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Operation label cannot be null or empty.", nameof(label));

        Kind = kind;
        InputIndexes = inputIndexes.ToArray();
        Id = id;
        Label = label;
    }

    public SeriesOperationKind Kind { get; }
    public IReadOnlyList<int> InputIndexes { get; }
    public string Id { get; }
    public string Label { get; }

    public static SeriesOperationRequest Identity(int index, string id, string label) =>
        new(SeriesOperationKind.Identity, [index], id, label);

    public static SeriesOperationRequest Normalize(int index, string id, string label) =>
        new(SeriesOperationKind.Normalize, [index], id, label);

    public static SeriesOperationRequest Sum(IReadOnlyList<int> indexes, string label = "Sum") =>
        new(SeriesOperationKind.Sum, indexes, "sum", label);

    public static SeriesOperationRequest Difference(int leftIndex, int rightIndex, string label) =>
        new(SeriesOperationKind.Difference, [leftIndex, rightIndex], "difference", label);

    public static SeriesOperationRequest Ratio(int leftIndex, int rightIndex, string label) =>
        new(SeriesOperationKind.Ratio, [leftIndex, rightIndex], "ratio", label);
}
