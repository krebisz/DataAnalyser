namespace DataVisualiser.VNext.Contracts;

public static class SeriesOperationRules
{
    public const string Lossless = "Lossless";
    public const string WindowedSmoothing = "WindowedSmoothing";

    public static int MinimumInputCount(SeriesOperationKind kind) =>
        kind == SeriesOperationKind.Sum ? 1 : RequiredInputCount(kind);

    public static int? MaximumInputCount(SeriesOperationKind kind) =>
        kind == SeriesOperationKind.Sum ? null : RequiredInputCount(kind);

    public static bool IsLossy(SeriesOperationKind kind) =>
        kind == SeriesOperationKind.MovingAverage;

    public static string DefaultLossiness(SeriesOperationKind kind) =>
        IsLossy(kind) ? WindowedSmoothing : Lossless;

    public static void Validate(SeriesOperationKind kind, IReadOnlyList<int> inputIndexes)
    {
        ArgumentNullException.ThrowIfNull(inputIndexes);

        var minimum = MinimumInputCount(kind);
        var maximum = MaximumInputCount(kind);

        if (inputIndexes.Count < minimum)
            throw new ArgumentException($"{kind} requires at least {minimum} input index(es).", nameof(inputIndexes));
        if (maximum.HasValue && inputIndexes.Count > maximum.Value)
            throw new ArgumentException($"{kind} requires exactly {maximum.Value} input index(es).", nameof(inputIndexes));
        if (inputIndexes.Any(index => index < 0))
            throw new ArgumentException($"{kind} input indexes cannot be negative.", nameof(inputIndexes));
    }

    private static int RequiredInputCount(SeriesOperationKind kind) =>
        kind switch
        {
            SeriesOperationKind.Identity => 1,
            SeriesOperationKind.Normalize => 1,
            SeriesOperationKind.Logarithm => 1,
            SeriesOperationKind.SquareRoot => 1,
            SeriesOperationKind.MovingAverage => 1,
            SeriesOperationKind.Difference => 2,
            SeriesOperationKind.Ratio => 2,
            SeriesOperationKind.Sum => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported operation kind.")
        };
}
