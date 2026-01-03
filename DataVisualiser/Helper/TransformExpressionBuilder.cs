using DataVisualiser.Models;

namespace DataVisualiser.Helper;

/// <summary>
///     Phase 4: Helper to build TransformExpressions from simple operation strings.
///     Bridges current simple operation model (string-based) to the new expression tree structure.
///     This allows current code to continue working while the infrastructure is ready for expansion.
/// </summary>
public static class TransformExpressionBuilder
{
    /// <summary>
    ///     Builds a TransformExpression from a simple operation string and metric indices.
    ///     Used by current implementation; can be extended for chained operations.
    /// </summary>
    /// <param name="operationId">Operation identifier (e.g., "Log", "Sqrt", "Add", "Subtract").</param>
    /// <param name="metricIndices">Indices of input metrics (0-based).</param>
    /// <returns>A TransformExpression representing the operation.</returns>
    public static TransformExpression? BuildFromOperation(string operationId, params int[] metricIndices)
    {
        var operation = TransformOperationRegistry.GetOperation(operationId);
        if (operation == null)
            return null;

        // Validate arity matches number of metrics
        if (operation.Arity > 0 && metricIndices.Length != operation.Arity)
            return null;

        // Build operands from metric indices
        var operands = metricIndices.Select(idx => TransformOperand.Metric(idx)).
            ToArray();

        // Create expression based on arity
        return operation.Arity switch
        {
            1 => TransformExpression.Unary(operation, operands[0]),
            2 => TransformExpression.Binary(operation, operands[0], operands[1]),
            _ => TransformExpression.CreateOperation(operation, operands)
        };
    }

    /// <summary>
    ///     Builds a chained expression (e.g., log(A + B)).
    /// </summary>
    /// <param name="outerOperationId">Outer operation (e.g., "Log").</param>
    /// <param name="innerOperationId">Inner operation (e.g., "Add").</param>
    /// <param name="metricIndices">Indices of input metrics for inner operation.</param>
    /// <returns>A chained TransformExpression.</returns>
    public static TransformExpression? BuildChained(string outerOperationId, string innerOperationId, params int[] metricIndices)
    {
        // Build inner expression first
        var innerExpr = BuildFromOperation(innerOperationId, metricIndices);
        if (innerExpr == null)
            return null;

        // Get outer operation
        var outerOp = TransformOperationRegistry.GetOperation(outerOperationId);
        if (outerOp == null || outerOp.Arity != 1)
            return null;

        // Chain: outer operation applied to inner expression result
        return TransformExpression.Unary(outerOp, TransformOperand.FromExpression(innerExpr));
    }

    /// <summary>
    ///     Builds an n-ary expression (e.g., Sum(A, B, C, D)).
    /// </summary>
    /// <param name="operationId">Operation identifier.</param>
    /// <param name="metricIndices">Indices of all input metrics.</param>
    /// <returns>An n-ary TransformExpression.</returns>
    public static TransformExpression? BuildNary(string operationId, params int[] metricIndices)
    {
        var operation = TransformOperationRegistry.GetOperation(operationId);
        if (operation == null)
            return null;

        var operands = metricIndices.Select(idx => TransformOperand.Metric(idx)).
            ToArray();
        return TransformExpression.CreateOperation(operation, operands);
    }
}