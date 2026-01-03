using DataVisualiser.Core.Transforms.Operations;

namespace DataVisualiser.Core.Transforms.Expressions;

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
