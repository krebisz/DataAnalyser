namespace DataVisualiser.Core.Transforms.Expressions;

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
