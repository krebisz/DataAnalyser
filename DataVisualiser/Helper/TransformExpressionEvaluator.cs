namespace DataVisualiser.Helper
{
    using DataVisualiser.Charts;
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

        /// <summary>
        /// Generates a label for transform operations using new infrastructure or fallback.
        /// </summary>
        /// <param name="operation">Operation identifier (e.g., "Log", "Sqrt", "Add", "Subtract").</param>
        /// <param name="metrics">List of metric data series.</param>
        /// <param name="ctx">Chart data context for metric labels.</param>
        /// <returns>Generated label string.</returns>
        public static string GenerateTransformLabel(
            string operation,
            IReadOnlyList<IReadOnlyList<HealthMetricData>> metrics,
            ChartDataContext? ctx)
        {
            var metricIndices = metrics.Count > 0 ? Enumerable.Range(0, metrics.Count).ToArray() : new[] { 0 };
            var expression = TransformExpressionBuilder.BuildFromOperation(operation, metricIndices);

            if (expression != null && metrics.Count > 0)
            {
                var metricLabels = BuildMetricLabelsFromContext(ctx, metrics.Count);
                var label = GenerateLabel(expression, metricLabels);
                System.Diagnostics.Debug.WriteLine($"[Transform] LABEL - Using NEW infrastructure label generation: '{label}'");
                return label;
            }

            // Fallback to simple label generation
            var legacyLabel = GenerateLegacyLabel(operation);
            System.Diagnostics.Debug.WriteLine($"[Transform] LABEL - Using LEGACY label generation: '{legacyLabel}'");
            return legacyLabel;
        }

        /// <summary>
        /// Builds metric labels from chart context, with fallback to generic labels.
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
            {
                metricLabels.Add($"Metric{metricLabels.Count}");
            }

            return metricLabels;
        }

        /// <summary>
        /// Generates a legacy label for an operation (fallback when expression building fails).
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
        /// Aligns two metric series by timestamp, keeping only points that exist in both.
        /// Required for transform expression evaluation which expects aligned data.
        /// </summary>
        public static (List<HealthMetricData>, List<HealthMetricData>) AlignMetricsByTimestamp(
            List<HealthMetricData> data1,
            List<HealthMetricData> data2)
        {
            var aligned1 = new List<HealthMetricData>();
            var aligned2 = new List<HealthMetricData>();

            var data2Lookup = data2.ToDictionary(d => d.NormalizedTimestamp, d => d);

            foreach (var point1 in data1)
            {
                if (data2Lookup.TryGetValue(point1.NormalizedTimestamp, out var point2))
                {
                    aligned1.Add(point1);
                    aligned2.Add(point2);
                }
            }

            return (aligned1, aligned2);
        }
    }
}
