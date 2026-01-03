namespace DataVisualiser.Models;

/// <summary>
///     Phase 4: Registry of available transform operations.
///     Provides a centralized way to register and retrieve transform operations.
///     Currently supports unary and binary operations; structure is provisioned for n-ary operations.
/// </summary>
public static class TransformOperationRegistry
{
    private static readonly Dictionary<string, TransformOperation> _operations = new();

    static TransformOperationRegistry()
    {
        // Register unary operations
        Register(TransformOperation.Unary("Log", "Logarithm", UnaryOperators.Logarithm));
        Register(TransformOperation.Unary("Sqrt", "Square Root", UnaryOperators.SquareRoot));

        // Register binary operations
        Register(TransformOperation.Binary("Add", "Add", BinaryOperators.Sum));
        Register(TransformOperation.Binary("Subtract", "Subtract", BinaryOperators.Difference));
        Register(TransformOperation.Binary("Divide", "Divide", BinaryOperators.Ratio));

        // Future: Register n-ary operations here
        // Example: Register(TransformOperation.Nary("Sum", "Sum", -1, values => values.Sum()));
    }

    /// <summary>
    ///     Registers a transform operation.
    /// </summary>
    public static void Register(TransformOperation operation)
    {
        if (operation == null || string.IsNullOrEmpty(operation.Id))
            throw new ArgumentException("Operation and Id must be provided.", nameof(operation));

        _operations[operation.Id] = operation;
    }

    /// <summary>
    ///     Gets an operation by its identifier.
    /// </summary>
    public static TransformOperation? GetOperation(string id)
    {
        return _operations.TryGetValue(id, out var op) ? op : null;
    }

    /// <summary>
    ///     Gets all registered operations.
    /// </summary>
    public static IReadOnlyList<TransformOperation> GetAllOperations()
    {
        return _operations.Values.ToList();
    }

    /// <summary>
    ///     Gets all unary operations.
    /// </summary>
    public static IReadOnlyList<TransformOperation> GetUnaryOperations()
    {
        return _operations.Values.Where(op => op.Arity == 1).
            ToList();
    }

    /// <summary>
    ///     Gets all binary operations.
    /// </summary>
    public static IReadOnlyList<TransformOperation> GetBinaryOperations()
    {
        return _operations.Values.Where(op => op.Arity == 2).
            ToList();
    }

    /// <summary>
    ///     Gets all n-ary operations (arity > 2 or variable).
    /// </summary>
    public static IReadOnlyList<TransformOperation> GetNaryOperations()
    {
        return _operations.Values.Where(op => op.Arity > 2 || op.Arity < 0).
            ToList();
    }
}