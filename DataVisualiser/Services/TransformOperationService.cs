using System.Diagnostics;
using DataVisualiser.Helper;
using DataVisualiser.Models;

namespace DataVisualiser.Services;

/// <summary>
///     Handles transform operations (unary and binary) on metric data.
///     Extracts complex transform logic from MainWindow.
/// </summary>
public sealed class TransformOperationService
{
    /// <summary>
    ///     Executes a unary transform operation (Log, Sqrt) on the provided data.
    /// </summary>
    public TransformOperationResult ComputeUnaryTransform(IEnumerable<HealthMetricData> data, string operation)
    {
        var allDataList = data.Where(d => d.Value.HasValue).
            OrderBy(d => d.NormalizedTimestamp).
            ToList();

        if (allDataList.Count == 0)
            return new TransformOperationResult
            {
                Success = false,
                Message = "No valid data points found"
            };

        // Phase 4: Use new transform expression infrastructure
        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0);
        List<double> computedResults;
        List<IReadOnlyList<HealthMetricData>> metricsList;

        if (expression == null)
        {
            // Fallback to legacy approach if operation not found in registry
            Debug.WriteLine($"[Transform] UNARY - Using LEGACY approach for operation: {operation}");
            var op = operation switch
            {
                "Log" => UnaryOperators.Logarithm,
                "Sqrt" => UnaryOperators.SquareRoot,
                _ => x => x
            };
            var allValues = allDataList.Select(d => (double)d.Value!.Value).
                ToList();
            computedResults = MathHelper.ApplyUnaryOperation(allValues, op);
            metricsList = new List<IReadOnlyList<HealthMetricData>>
            {
                allDataList
            };
        }
        else
        {
            // Evaluate using new infrastructure
            Debug.WriteLine($"[Transform] UNARY - Using NEW infrastructure for operation: {operation}, expression built successfully");
            metricsList = new List<IReadOnlyList<HealthMetricData>>
            {
                allDataList
            };
            computedResults = TransformExpressionEvaluator.Evaluate(expression, metricsList);
            Debug.WriteLine($"[Transform] UNARY - Evaluated {computedResults.Count} results using TransformExpressionEvaluator");
        }

        return new TransformOperationResult
        {
            Success = true,
            DataList = allDataList,
            ComputedResults = computedResults,
            MetricsList = metricsList,
            Operation = operation
        };
    }

    /// <summary>
    ///     Executes a binary transform operation (Add, Subtract) on two data series.
    /// </summary>
    public TransformOperationResult ComputeBinaryTransform(IEnumerable<HealthMetricData> data1, IEnumerable<HealthMetricData> data2, string operation)
    {
        var allData1List = data1.Where(d => d.Value.HasValue).
            OrderBy(d => d.NormalizedTimestamp).
            ToList();

        var allData2List = data2.Where(d => d.Value.HasValue).
            OrderBy(d => d.NormalizedTimestamp).
            ToList();

        if (allData1List.Count == 0 || allData2List.Count == 0)
            return new TransformOperationResult
            {
                Success = false,
                Message = "One or both data series are empty"
            };

        // Phase 4: Align data by timestamp (required for expression evaluator)
        var alignedData = TransformExpressionEvaluator.AlignMetricsByTimestamp(allData1List, allData2List);
        if (alignedData.Item1.Count == 0 || alignedData.Item2.Count == 0)
            return new TransformOperationResult
            {
                Success = false,
                Message = "No aligned data points found after timestamp alignment"
            };

        // Phase 4: Use new transform expression infrastructure
        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0, 1);
        List<double> binaryComputedResults;
        List<IReadOnlyList<HealthMetricData>> binaryMetricsList;

        if (expression == null)
        {
            // Fallback to legacy approach if operation not found in registry
            Debug.WriteLine($"[Transform] BINARY - Using LEGACY approach for operation: {operation}");
            var op = operation switch
            {
                "Add" => BinaryOperators.Sum,
                "Subtract" => BinaryOperators.Difference,
                _ => (a, b) => a
            };

            var allValues1 = alignedData.Item1.Select(d => (double)d.Value!.Value).
                ToList();
            var allValues2 = alignedData.Item2.Select(d => (double)d.Value!.Value).
                ToList();
            binaryComputedResults = MathHelper.ApplyBinaryOperation(allValues1, allValues2, op);
            binaryMetricsList = new List<IReadOnlyList<HealthMetricData>>
            {
                alignedData.Item1,
                alignedData.Item2
            };
            Debug.WriteLine($"[Transform] BINARY - Legacy computation completed: {binaryComputedResults.Count} results");
        }
        else
        {
            // Evaluate using new infrastructure
            Debug.WriteLine($"[Transform] BINARY - Using NEW infrastructure for operation: {operation}, expression built successfully");
            binaryMetricsList = new List<IReadOnlyList<HealthMetricData>>
            {
                alignedData.Item1,
                alignedData.Item2
            };
            binaryComputedResults = TransformExpressionEvaluator.Evaluate(expression, binaryMetricsList);
            Debug.WriteLine($"[Transform] BINARY - Evaluated {binaryComputedResults.Count} results using TransformExpressionEvaluator");
        }

        return new TransformOperationResult
        {
            Success = true,
            DataList = alignedData.Item1,
            ComputedResults = binaryComputedResults,
            MetricsList = binaryMetricsList,
            Operation = operation
        };
    }
}