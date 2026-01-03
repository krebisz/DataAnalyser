namespace DataVisualiser.Models;

/// <summary>
///     Phase 4: Represents a transform expression that can handle multiple metrics and chained operations.
///     This structure is provisioned for future expansion to support:
///     - N metrics (not just 1 or 2)
///     - Chained operations (e.g., log(A + B), sqrt(A - B + C))
///     - Complex expression trees
///     Transform results are explicitly ephemeral and non-canonical.
/// </summary>
public sealed class TransformExpression
{
    /// <summary>
    ///     The operation to apply. Null for leaf nodes (direct metric values).
    /// </summary>
    public TransformOperation? Operation { get; init; }

    /// <summary>
    ///     Operands for this expression. Can be:
    ///     - Other TransformExpressions (for chained operations)
    ///     - Metric indices (0-based, referring to input metrics)
    /// </summary>
    public List<TransformOperand> Operands { get; init; } = new();

    /// <summary>
    ///     Creates a leaf expression representing a direct metric value.
    /// </summary>
    public static TransformExpression Metric(int metricIndex)
    {
        return new TransformExpression
        {
            Operation = null,
            Operands = new List<TransformOperand>
            {
                new()
                {
                    MetricIndex = metricIndex
                }
            }
        };
    }

    /// <summary>
    ///     Creates an operation expression with the specified operands.
    /// </summary>
    public static TransformExpression CreateOperation(TransformOperation operation, params TransformOperand[] operands)
    {
        return new TransformExpression
        {
            Operation = operation,
            Operands = operands.ToList()
        };
    }

    /// <summary>
    ///     Creates a unary operation expression.
    /// </summary>
    public static TransformExpression Unary(TransformOperation operation, TransformOperand operand)
    {
        return new TransformExpression
        {
            Operation = operation,
            Operands = new List<TransformOperand>
            {
                operand
            }
        };
    }

    /// <summary>
    ///     Creates a binary operation expression.
    /// </summary>
    public static TransformExpression Binary(TransformOperation operation, TransformOperand left, TransformOperand right)
    {
        return new TransformExpression
        {
            Operation = operation,
            Operands = new List<TransformOperand>
            {
                left,
                right
            }
        };
    }
}

/// <summary>
///     Represents an operand in a transform expression.
///     Can be either a metric reference or a nested expression.
/// </summary>
public sealed class TransformOperand
{
    /// <summary>
    ///     Index of the input metric (0-based). Set when operand is a direct metric reference.
    /// </summary>
    public int? MetricIndex { get; init; }

    /// <summary>
    ///     Nested expression. Set when operand is a computed result from another operation.
    /// </summary>
    public TransformExpression? Expression { get; init; }

    /// <summary>
    ///     Creates a metric operand.
    /// </summary>
    public static TransformOperand Metric(int metricIndex)
    {
        return new TransformOperand
        {
            MetricIndex = metricIndex
        };
    }

    /// <summary>
    ///     Creates an expression operand (for chaining).
    /// </summary>
    public static TransformOperand FromExpression(TransformExpression expression)
    {
        return new TransformOperand
        {
            Expression = expression
        };
    }
}

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