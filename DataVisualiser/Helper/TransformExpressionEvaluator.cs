namespace DataVisualiser.Helper
{
    using DataVisualiser.Models;
    using System.Linq;

    /// <summary>
    /// Phase 4: Evaluates transform expressions over metric data.
    /// 
    /// Supports:
    /// - Multiple input metrics
    /// - Chained operations (nested expressions)
    /// - Unary, binary, and n-ary operations
    /// 
    /// Structure is provisioned for future expansion to complex expression trees.
    /// </summary>
    public static class TransformExpressionEvaluator
    {
        /// <summary>
        /// Evaluates a transform expression over aligned metric data.
        /// 
        /// All input metrics must be aligned by timestamp (same length, same timestamps).
        /// </summary>
        /// <param name="expression">The transform expression to evaluate.</param>
        /// <param name="metrics">List of metric data series, each aligned by timestamp.</param>
        /// <returns>List of computed values, one per timestamp.</returns>
        public static List<double> Evaluate(
            TransformExpression expression,
            IReadOnlyList<IReadOnlyList<HealthMetricData>> metrics)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            if (metrics == null || metrics.Count == 0)
                throw new ArgumentException("At least one metric series is required.", nameof(metrics));

            // Validate all metrics have the same length (aligned)
            var firstLength = metrics[0].Count;
            if (metrics.Any(m => m.Count != firstLength))
                throw new ArgumentException("All metric series must be aligned (same length).", nameof(metrics));

            // Evaluate expression for each timestamp position
            var results = new List<double>(firstLength);
            for (int i = 0; i < firstLength; i++)
            {
                var result = EvaluateAt(expression, metrics, i);
                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Evaluates the expression at a specific timestamp index.
        /// </summary>
        private static double EvaluateAt(
            TransformExpression expression,
            IReadOnlyList<IReadOnlyList<HealthMetricData>> metrics,
            int index)
        {
            // Leaf node: return metric value directly
            if (expression.Operation == null)
            {
                if (expression.Operands.Count != 1 || expression.Operands[0].MetricIndex == null)
                    return double.NaN;

                var metricIndex = expression.Operands[0].MetricIndex.Value;
                if (metricIndex < 0 || metricIndex >= metrics.Count)
                    return double.NaN;

                var metric = metrics[metricIndex];
                if (index >= metric.Count)
                    return double.NaN;

                var dataPoint = metric[index];
                return dataPoint.Value.HasValue ? (double)dataPoint.Value.Value : double.NaN;
            }

            // Operation node: evaluate operands first, then apply operation
            var operandValues = new List<double>();
            foreach (var operand in expression.Operands)
            {
                double value;
                if (operand.MetricIndex.HasValue)
                {
                    // Direct metric reference
                    var metricIndex = operand.MetricIndex.Value;
                    if (metricIndex < 0 || metricIndex >= metrics.Count || index >= metrics[metricIndex].Count)
                    {
                        value = double.NaN;
                    }
                    else
                    {
                        var dataPoint = metrics[metricIndex][index];
                        value = dataPoint.Value.HasValue ? (double)dataPoint.Value.Value : double.NaN;
                    }
                }
                else if (operand.Expression != null)
                {
                    // Nested expression (chaining)
                    value = EvaluateAt(operand.Expression, metrics, index);
                }
                else
                {
                    value = double.NaN;
                }

                operandValues.Add(value);
            }

            // Apply operation
            return expression.Operation.Execute(operandValues);
        }

        /// <summary>
        /// Generates a human-readable label for a transform expression.
        /// </summary>
        public static string GenerateLabel(
            TransformExpression expression,
            IReadOnlyList<string> metricLabels)
        {
            if (expression == null)
                return "Transform Result";

            // Leaf node: return metric label
            if (expression.Operation == null)
            {
                if (expression.Operands.Count == 1 && expression.Operands[0].MetricIndex.HasValue)
                {
                    var idx = expression.Operands[0].MetricIndex.Value;
                    return idx >= 0 && idx < metricLabels.Count ? metricLabels[idx] : $"Metric[{idx}]";
                }
                return "Metric";
            }

            // Operation node: build label from operation and operands
            var operandLabels = expression.Operands.Select(op =>
            {
                if (op.MetricIndex.HasValue)
                {
                    var idx = op.MetricIndex.Value;
                    return idx >= 0 && idx < metricLabels.Count ? metricLabels[idx] : $"Metric[{idx}]";
                }
                if (op.Expression != null)
                {
                    return $"({GenerateLabel(op.Expression, metricLabels)})";
                }
                return "?";
            }).ToList();

            var operationSymbol = GetOperationSymbol(expression.Operation.Id);
            var label = string.Join($" {operationSymbol} ", operandLabels);

            return $"[Transform] {label}";
        }

        /// <summary>
        /// Gets the display symbol for an operation.
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
    }
}
