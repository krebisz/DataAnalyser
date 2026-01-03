namespace DataVisualiser.Core.Transforms.Operations;

/// <summary>
///     Represents a transform operation with its arity and execution function.
/// </summary>
public sealed class TransformOperation
{
    /// <summary>
    ///     Operation identifier (e.g., "Log", "Sqrt", "Add", "Subtract").
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    ///     Display name for the operation.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    ///     Number of operands required (1 for unary, 2 for binary, N for n-ary).
    /// </summary>
    public int Arity { get; init; }

    /// <summary>
    ///     Function to execute the operation. Takes a list of operand values and returns the result.
    /// </summary>
    public Func<IReadOnlyList<double>, double> Execute { get; init; } = _ => double.NaN;

    /// <summary>
    ///     Creates a unary operation.
    /// </summary>
    public static TransformOperation Unary(string id, string displayName, Func<double, double> operation)
    {
        return new TransformOperation
        {
                Id = id,
                DisplayName = displayName,
                Arity = 1,
                Execute = values => values.Count >= 1 ? operation(values[0]) : double.NaN
        };
    }

    /// <summary>
    ///     Creates a binary operation.
    /// </summary>
    public static TransformOperation Binary(string id, string displayName, Func<double, double, double> operation)
    {
        return new TransformOperation
        {
                Id = id,
                DisplayName = displayName,
                Arity = 2,
                Execute = values => values.Count >= 2 ? operation(values[0], values[1]) : double.NaN
        };
    }

    /// <summary>
    ///     Creates an n-ary operation (for future expansion).
    /// </summary>
    public static TransformOperation Nary(string id, string displayName, int arity, Func<IReadOnlyList<double>, double> operation)
    {
        return new TransformOperation
        {
                Id = id,
                DisplayName = displayName,
                Arity = arity,
                Execute = operation
        };
    }
}