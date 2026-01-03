using System.Diagnostics;
using DataVisualiser.Core.Transforms.Evaluators;
using DataVisualiser.Core.Transforms.Expressions;
using DataVisualiser.Core.Transforms.Operations;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Transforms;

/// <summary>
///     Handles transform computation logic, extracting complex transform operations
///     from MainWindow to improve maintainability and testability.
/// </summary>
public sealed class TransformComputationService
{
    /// <summary>
    ///     Computes a unary transform operation (Log, Sqrt) on the provided data.
    /// </summary>
    public TransformComputationResult ComputeUnaryTransform(IEnumerable<MetricData> data, string operation)
    {
        var allDataList = PrepareMetricData(data);
        if (allDataList.Count == 0)
            return TransformComputationResult.Empty;

        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0);

        var metricsList = new List<IReadOnlyList<MetricData>>
        {
                allDataList
        };

        var computedResults = expression != null ? EvaluateWithExpression(expression, metricsList, operation, true) : EvaluateUnaryLegacy(allDataList, operation);

        return BuildResult(allDataList, computedResults, operation, metricsList);
    }


    /// <summary>
    ///     Computes a binary transform operation (Add, Subtract) on the provided data.
    /// </summary>
    public TransformComputationResult ComputeBinaryTransform(IEnumerable<MetricData> data1, IEnumerable<MetricData> data2, string operation)
    {
        var list1 = PrepareMetricData(data1);
        var list2 = PrepareMetricData(data2);

        if (list1.Count == 0 || list2.Count == 0)
            return TransformComputationResult.Empty;

        var (aligned1, aligned2) = TransformExpressionEvaluator.AlignMetricsByTimestamp(list1, list2);

        if (aligned1.Count == 0 || aligned2.Count == 0)
            return TransformComputationResult.Empty;

        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0, 1);

        var metricsList = new List<IReadOnlyList<MetricData>>
        {
                aligned1,
                aligned2
        };

        var computedResults = expression != null ? EvaluateWithExpression(expression, metricsList, operation, false) : EvaluateBinaryLegacy(aligned1, aligned2, operation);

        return BuildResult(aligned1, computedResults, operation, metricsList);
    }


    private static List<MetricData> PrepareMetricData(IEnumerable<MetricData> data)
    {
        return data.Where(d => d.Value.HasValue).
                    OrderBy(d => d.NormalizedTimestamp).
                    ToList();
    }

    private static List<double> EvaluateWithExpression(TransformExpression expression, List<IReadOnlyList<MetricData>> metricsList, string operation, bool isUnary)
    {
        Debug.WriteLine($"[Transform] {(isUnary ? "UNARY" : "BINARY")} - Using NEW infrastructure for operation: {operation}");

        var results = TransformExpressionEvaluator.Evaluate(expression, metricsList);

        Debug.WriteLine($"[Transform] {(isUnary ? "UNARY" : "BINARY")} - Evaluated {results.Count} results using TransformExpressionEvaluator");

        return results;
    }

    private static List<double> EvaluateUnaryLegacy(IReadOnlyList<MetricData> data, string operation)
    {
        Debug.WriteLine($"[Transform] UNARY - Using LEGACY approach for operation: {operation}");

        var op = operation switch
        {
                "Log"  => UnaryOperators.Logarithm,
                "Sqrt" => UnaryOperators.SquareRoot,
                _      => x => x
        };

        var values = data.Select(d => (double)d.Value!.Value).
                          ToList();

        return MathHelper.ApplyUnaryOperation(values, op);
    }

    private static List<double> EvaluateBinaryLegacy(IReadOnlyList<MetricData> data1, IReadOnlyList<MetricData> data2, string operation)
    {
        Debug.WriteLine($"[Transform] BINARY - Using LEGACY approach for operation: {operation}");

        var op = operation switch
        {
                "Add"      => BinaryOperators.Sum,
                "Subtract" => BinaryOperators.Difference,
                _          => (a, b) => a
        };

        var values1 = data1.Select(d => (double)d.Value!.Value).
                            ToList();
        var values2 = data2.Select(d => (double)d.Value!.Value).
                            ToList();

        return MathHelper.ApplyBinaryOperation(values1, values2, op);
    }

    private static TransformComputationResult BuildResult(IReadOnlyList<MetricData> dataList, List<double> computedResults, string operation, List<IReadOnlyList<MetricData>> metricsList)
    {
        return new TransformComputationResult
        {
                DataList = dataList.ToList(),
                ComputedResults = computedResults,
                Operation = operation,
                MetricsList = metricsList,
                IsSuccess = true
        };
    }
}