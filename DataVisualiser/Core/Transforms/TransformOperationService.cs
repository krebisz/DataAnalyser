using System.Diagnostics;
using DataVisualiser.Core.Transforms.Evaluators;
using DataVisualiser.Core.Transforms.Expressions;
using DataVisualiser.Core.Transforms.Operations;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Transforms;

/// <summary>
///     Handles transform operations (unary and binary) on metric data.
///     Extracts complex transform logic from MainWindow.
/// </summary>
public sealed class TransformOperationService
{
    /// <summary>
    ///     Executes a unary transform operation (Log, Sqrt) on the provided data.
    /// </summary>
    public TransformOperationResult ComputeUnaryTransform(IEnumerable<MetricData> data, string operation)
    {
        var preparedData = data.Where(d => d.Value.HasValue).
                                OrderBy(d => d.NormalizedTimestamp).
                                ToList();

        if (preparedData.Count == 0)
            return new TransformOperationResult
            {
                    Success = false,
                    Message = "No valid data points found"
            };

        var metricsList = new List<IReadOnlyList<MetricData>>
        {
                preparedData
        };

        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0);
        List<double> computedResults;

        if (expression != null)
        {
            Debug.WriteLine($"[Transform] UNARY - Using NEW infrastructure for operation: {operation}");

            computedResults = TransformExpressionEvaluator.Evaluate(expression, metricsList);

            Debug.WriteLine($"[Transform] UNARY - Evaluated {computedResults.Count} results");
        }
        else
        {
            Debug.WriteLine($"[Transform] UNARY - Using LEGACY approach for operation: {operation}");

            var op = operation switch
            {
                    "Log"  => UnaryOperators.Logarithm,
                    "Sqrt" => UnaryOperators.SquareRoot,
                    _      => x => x
            };

            var values = preparedData.Select(d => (double)d.Value!.Value).
                                      ToList();

            computedResults = MathHelper.ApplyUnaryOperation(values, op);
        }

        return new TransformOperationResult
        {
                Success = true,
                DataList = preparedData,
                ComputedResults = computedResults,
                MetricsList = metricsList,
                Operation = operation
        };
    }


    /// <summary>
    ///     Executes a binary transform operation (Add, Subtract) on two data series.
    /// </summary>
    public TransformOperationResult ComputeBinaryTransform(IEnumerable<MetricData> data1, IEnumerable<MetricData> data2, string operation)
    {
        var prepared1 = data1.Where(d => d.Value.HasValue).
                              OrderBy(d => d.NormalizedTimestamp).
                              ToList();

        var prepared2 = data2.Where(d => d.Value.HasValue).
                              OrderBy(d => d.NormalizedTimestamp).
                              ToList();

        if (prepared1.Count == 0 || prepared2.Count == 0)
            return new TransformOperationResult
            {
                    Success = false,
                    Message = "One or both data series are empty"
            };

        var (aligned1, aligned2) = TransformExpressionEvaluator.AlignMetricsByTimestamp(prepared1, prepared2);

        if (aligned1.Count == 0 || aligned2.Count == 0)
            return new TransformOperationResult
            {
                    Success = false,
                    Message = "No aligned data points found after timestamp alignment"
            };

        var metricsList = new List<IReadOnlyList<MetricData>>
        {
                aligned1,
                aligned2
        };

        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0, 1);
        List<double> computedResults;

        if (expression != null)
        {
            Debug.WriteLine($"[Transform] BINARY - Using NEW infrastructure for operation: {operation}");

            computedResults = TransformExpressionEvaluator.Evaluate(expression, metricsList);

            Debug.WriteLine($"[Transform] BINARY - Evaluated {computedResults.Count} results");
        }
        else
        {
            Debug.WriteLine($"[Transform] BINARY - Using LEGACY approach for operation: {operation}");

            var op = operation switch
            {
                    "Add"      => BinaryOperators.Sum,
                    "Subtract" => BinaryOperators.Difference,
                    _          => (a, b) => a
            };

            var values1 = aligned1.Select(d => (double)d.Value!.Value).
                                   ToList();
            var values2 = aligned2.Select(d => (double)d.Value!.Value).
                                   ToList();

            computedResults = MathHelper.ApplyBinaryOperation(values1, values2, op);
        }

        return new TransformOperationResult
        {
                Success = true,
                DataList = aligned1,
                ComputedResults = computedResults,
                MetricsList = metricsList,
                Operation = operation
        };
    }
}