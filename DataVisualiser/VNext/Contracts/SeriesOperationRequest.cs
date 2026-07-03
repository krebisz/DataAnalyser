using DataVisualiser.Shared.Models;

namespace DataVisualiser.VNext.Contracts;

public enum SeriesOperationKind
{
    Identity = 0,
    Normalize = 1,
    Sum = 2,
    Difference = 3,
    Ratio = 4,
    Logarithm = 5,
    SquareRoot = 6,
    MovingAverage = 7
}

public sealed record SeriesOperationRequest
{
    public SeriesOperationRequest(
        SeriesOperationKind kind,
        IReadOnlyList<int> inputIndexes,
        string id,
        string label)
    {
        SeriesOperationRules.Validate(kind, inputIndexes);
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
    public int WindowSize { get; init; }
    public NormalizationMode? NormalizationMode { get; init; }
    public IReadOnlyList<int> NormalizationReferenceIndexes { get; init; } = [];

    public static SeriesOperationRequest Identity(int index, string id, string label) =>
        new(SeriesOperationKind.Identity, [index], id, label);

    public static SeriesOperationRequest Normalize(int index, string id, string label) =>
        new(SeriesOperationKind.Normalize, [index], id, label);

    public static SeriesOperationRequest Normalize(
        int index,
        string id,
        string label,
        NormalizationMode mode,
        IReadOnlyList<int>? referenceIndexes = null)
    {
        var references = referenceIndexes?.ToArray() ?? [index];
        if (references.Length == 0)
            throw new ArgumentException("Normalization requires at least one reference index.", nameof(referenceIndexes));
        if (references.Any(reference => reference < 0))
            throw new ArgumentException("Normalization reference indexes cannot be negative.", nameof(referenceIndexes));
        if (!references.Contains(index))
            throw new ArgumentException("Normalization reference indexes must include the normalized input index.", nameof(referenceIndexes));

        return new SeriesOperationRequest(SeriesOperationKind.Normalize, [index], id, label)
        {
            NormalizationMode = mode,
            NormalizationReferenceIndexes = references
        };
    }

    public static SeriesOperationRequest Logarithm(int index, string id, string label) =>
        new(SeriesOperationKind.Logarithm, [index], id, label);

    public static SeriesOperationRequest SquareRoot(int index, string id, string label) =>
        new(SeriesOperationKind.SquareRoot, [index], id, label);

    public static SeriesOperationRequest Sum(IReadOnlyList<int> indexes, string label = "Sum") =>
        new(SeriesOperationKind.Sum, indexes, "sum", label);

    public static SeriesOperationRequest Difference(int leftIndex, int rightIndex, string label) =>
        new(SeriesOperationKind.Difference, [leftIndex, rightIndex], "difference", label);

    public static SeriesOperationRequest Ratio(int leftIndex, int rightIndex, string label) =>
        new(SeriesOperationKind.Ratio, [leftIndex, rightIndex], "ratio", label);

    public static SeriesOperationRequest MovingAverage(int index, int windowSize, string id, string label) =>
        new(SeriesOperationKind.MovingAverage, [index], id, label) { WindowSize = windowSize };
}
