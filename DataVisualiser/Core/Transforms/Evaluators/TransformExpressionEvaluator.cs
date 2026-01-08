using System.Diagnostics;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Transforms.Expressions;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Transforms.Evaluators;

/// <summary>
///     Phase 4: Evaluates transform expressions over metric data.
///     Supports:
///     - Multiple input metrics
///     - Chained operations (nested expressions)
///     - Unary, binary, and n-ary operations
///     Structure is provisioned for future expansion to complex expression trees.
/// </summary>
public static class TransformExpressionEvaluator
{
    /// <summary>
    ///     Evaluates a transform expression over aligned metric data.
    ///     All input metrics must be aligned by timestamp (same length, same timestamps).
    /// </summary>
    /// <param name="expression">The transform expression to evaluate.</param>
    /// <param name="metrics">List of metric data series, each aligned by timestamp.</param>
    /// <returns>List of computed values, one per timestamp.</returns>
    public static List<double> Evaluate(TransformExpression expression, IReadOnlyList<IReadOnlyList<MetricData>> metrics)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        if (metrics == null || metrics.Count == 0)
            throw new ArgumentException("At least one metric series is required.", nameof(metrics));

        var length = ValidateAlignedMetrics(metrics);

        var results = new List<double>(length);
        for (var i = 0; i < length; i++)
            results.Add(EvaluateAt(expression, metrics, i));

        return results;
    }

    private static int ValidateAlignedMetrics(IReadOnlyList<IReadOnlyList<MetricData>> metrics)
    {
        var length = metrics[0].Count;
        if (metrics.Any(m => m.Count != length))
            throw new ArgumentException("All metric series must be aligned (same length).", nameof(metrics));

        return length;
    }

    /// <summary>
    ///     Evaluates the expression at a specific timestamp index.
    /// </summary>
    private static double EvaluateAt(TransformExpression expression, IReadOnlyList<IReadOnlyList<MetricData>> metrics, int index)
    {
        if (expression.Operation == null)
            return ResolveLeafValue(expression, metrics, index);

        var operandValues = EvaluateOperands(expression, metrics, index);
        return expression.Operation.Execute(operandValues);
    }


    private static double ResolveLeafValue(TransformExpression expression, IReadOnlyList<IReadOnlyList<MetricData>> metrics, int index)
    {
        var metricIndex = expression.Operands[0].MetricIndex;
        if (expression.Operands.Count != 1 || metricIndex == null)
            return double.NaN;

        return ResolveMetricValue(metrics, metricIndex.Value, index);
    }

    private static List<double> EvaluateOperands(TransformExpression expression, IReadOnlyList<IReadOnlyList<MetricData>> metrics, int index)
    {
        var values = new List<double>(expression.Operands.Count);

        foreach (var operand in expression.Operands)
            values.Add(EvaluateOperand(operand, metrics, index));

        return values;
    }

    private static double EvaluateOperand(TransformOperand operand, IReadOnlyList<IReadOnlyList<MetricData>> metrics, int index)
    {
        if (operand.MetricIndex.HasValue)
            return ResolveMetricValue(metrics, operand.MetricIndex.Value, index);

        if (operand.Expression != null)
            return EvaluateAt(operand.Expression, metrics, index);

        return double.NaN;
    }

    private static double ResolveMetricValue(IReadOnlyList<IReadOnlyList<MetricData>> metrics, int metricIndex, int index)
    {
        if (metricIndex < 0 || metricIndex >= metrics.Count || index >= metrics[metricIndex].Count)
            return double.NaN;

        var dataPoint = metrics[metricIndex][index];
        return dataPoint.Value.HasValue ? (double)dataPoint.Value.Value : double.NaN;
    }

    public static string GenerateLabel(TransformExpression expression, IReadOnlyList<string> metricLabels)
    {
        if (expression == null)
            return "Transform Result";

        var label = BuildLabel(expression, metricLabels);
        return expression.Operation == null ? label : $"[Transform] {label}";
    }

    /// <summary>
    ///     Generates a human-readable label for a transform expression.
    /// </summary>
    private static string BuildLabel(TransformExpression expression, IReadOnlyList<string> metricLabels)
    {
        if (expression.Operation == null)
            return ResolveMetricLabel(expression, metricLabels);

        var operandLabels = expression.Operands.Select(op => ResolveOperandLabel(op, metricLabels)).ToList();

        var symbol = GetOperationSymbol(expression.Operation.Id);
        return string.Join($" {symbol} ", operandLabels);
    }

    private static string ResolveOperandLabel(TransformOperand operand, IReadOnlyList<string> metricLabels)
    {
        if (operand.MetricIndex.HasValue)
        {
            var idx = operand.MetricIndex.Value;
            return idx >= 0 && idx < metricLabels.Count ? metricLabels[idx] : $"Metric[{idx}]";
        }

        if (operand.Expression != null)
            return $"({BuildLabel(operand.Expression, metricLabels)})";

        return "?";
    }

    private static string ResolveMetricLabel(TransformExpression expression, IReadOnlyList<string> metricLabels)
    {
        var metricIndex = expression.Operands[0].MetricIndex;
        if (expression.Operands.Count == 1 && metricIndex.HasValue)
        {
            var idx = metricIndex.Value;
            return idx >= 0 && idx < metricLabels.Count ? metricLabels[idx] : $"Metric[{idx}]";
        }

        return "Metric";
    }


    /// <summary>
    ///     Gets the display symbol for an operation.
    /// </summary>
    private static string GetOperationSymbol(string operationId)
    {
        return operationId switch
        {
                "Log" => "log",
                "Sqrt" => "√",
                "Add" => "+",
                "Subtract" => "-",
                _ => operationId
        };
    }

    /// <summary>
    ///     Generates a label for transform operations using new infrastructure or fallback.
    /// </summary>
    /// <param name="operation">Operation identifier (e.g., "Log", "Sqrt", "Add", "Subtract").</param>
    /// <param name="metrics">List of metric data series.</param>
    /// <param name="ctx">Chart data context for metric labels.</param>
    /// <returns>Generated label string.</returns>
    public static string GenerateTransformLabel(string operation, IReadOnlyList<IReadOnlyList<MetricData>> metrics, ChartDataContext? ctx)
    {
        var metricIndices = metrics.Count > 0 ?
                Enumerable.Range(0, metrics.Count).ToArray() :
                new[]
                {
                        0
                };
        var expression = TransformExpressionBuilder.BuildFromOperation(operation, metricIndices);

        if (expression != null && metrics.Count > 0)
        {
            var metricLabels = BuildMetricLabelsFromContext(ctx, metrics.Count);
            var label = GenerateLabel(expression, metricLabels);
            Debug.WriteLine($"[Transform] LABEL - Using NEW infrastructure label generation: '{label}'");
            return label;
        }

        // Fallback to simple label generation
        var legacyLabel = GenerateLegacyLabel(operation);
        Debug.WriteLine($"[Transform] LABEL - Using LEGACY label generation: '{legacyLabel}'");
        return legacyLabel;
    }

    /// <summary>
    ///     Builds metric labels from chart context, with fallback to generic labels.
    /// </summary>
    public static List<string> BuildMetricLabelsFromContext(ChartDataContext? ctx, int requiredCount)
    {
        var metricLabels = new List<string>();

        if (ctx != null)
        {
            if (!string.IsNullOrEmpty(ctx.PrimarySubtype))
                metricLabels.Add($"{ctx.MetricType}:{ctx.PrimarySubtype}");
            else if (!string.IsNullOrEmpty(ctx.MetricType))
                metricLabels.Add(ctx.MetricType);

            if (requiredCount > 1 && !string.IsNullOrEmpty(ctx.SecondarySubtype))
                metricLabels.Add($"{ctx.MetricType}:{ctx.SecondarySubtype}");
        }

        // Fallback to generic labels if context not available
        while (metricLabels.Count < requiredCount)
            metricLabels.Add($"Metric{metricLabels.Count}");

        return metricLabels;
    }

    /// <summary>
    ///     Generates a legacy label for an operation (fallback when expression building fails).
    /// </summary>
    private static string GenerateLegacyLabel(string operationTag)
    {
        return operationTag switch
        {
                "Log" => "Log(Result)",
                "Sqrt" => "√(Result)",
                "Add" => "Result (Sum)",
                "Subtract" => "Result (Difference)",
                _ => "Transform Result"
        };
    }

    /// <summary>
    ///     Aligns two metric series by timestamp, keeping only points that exist in both.
    ///     Required for transform expression evaluation which expects aligned data.
    /// </summary>
    public static(List<MetricData>, List<MetricData>) AlignMetricsByTimestamp(List<MetricData> data1, List<MetricData> data2)
    {
        var aligned1 = new List<MetricData>();
        var aligned2 = new List<MetricData>();

        var data2Lookup = data2.ToDictionary(d => d.NormalizedTimestamp, d => d);

        foreach (var point1 in data1)
            if (data2Lookup.TryGetValue(point1.NormalizedTimestamp, out var point2))
            {
                aligned1.Add(point1);
                aligned2.Add(point2);
            }

        return (aligned1, aligned2);
    }

    /// <summary>
    ///     Creates result data objects for transform grid display.
    /// </summary>
    public static List<object> CreateTransformResultData(List<MetricData> dataList, List<double> results)
    {
        return dataList.Zip(results,
                               (d, r) => new
                               {
                                       Timestamp = d.NormalizedTimestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                                       Value = double.IsNaN(r) ? "NaN" : r.ToString("F4")
                               })
                       .ToList<object>();
    }
}