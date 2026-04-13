using DataVisualiser.Core.Transforms;
using DataVisualiser.Core.Transforms.Expressions;
using DataVisualiser.Core.Transforms.Operations;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.MainHost;

internal static class EvidenceTransformParityComputer
{
    internal static bool IsUnaryTransform(string operation)
    {
        return string.Equals(operation, "Log", StringComparison.OrdinalIgnoreCase)
               || string.Equals(operation, "Sqrt", StringComparison.OrdinalIgnoreCase);
    }

    internal static (ParityResultSnapshot Result, int LegacySamples, int NewSamples, bool ExpressionAvailable) ComputeUnary(IReadOnlyList<MetricData> data, string operation)
    {
        var prepared = data.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
        if (prepared.Count == 0)
            return (new ParityResultSnapshot { Passed = false, Error = "No valid data points" }, 0, 0, false);

        var values = prepared.Select(d => (double)d.Value!.Value).ToList();
        var legacyOp = operation switch
        {
            "Log" => UnaryOperators.Logarithm,
            "Sqrt" => UnaryOperators.SquareRoot,
            _ => x => x
        };
        var legacy = MathHelper.ApplyUnaryOperation(values, legacyOp);
        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0);
        var modern = expression != null ? TransformExpressionEvaluator.Evaluate(expression, [prepared]) : legacy;
        return (CompareResults(legacy, modern), legacy.Count, modern.Count, expression != null);
    }

    internal static (ParityResultSnapshot Result, int LegacySamples, int NewSamples, bool ExpressionAvailable) ComputeBinary(IReadOnlyList<MetricData> data1, IReadOnlyList<MetricData> data2, string operation)
    {
        var prepared1 = data1.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
        var prepared2 = data2.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
        var (aligned1, aligned2) = TransformExpressionEvaluator.AlignMetricsByTimestamp(prepared1, prepared2);
        if (aligned1.Count == 0 || aligned2.Count == 0)
            return (new ParityResultSnapshot { Passed = false, Error = "No aligned data points" }, 0, 0, false);

        var values1 = aligned1.Select(d => (double)d.Value!.Value).ToList();
        var values2 = aligned2.Select(d => (double)d.Value!.Value).ToList();
        var legacyOp = operation switch
        {
            "Add" => BinaryOperators.Sum,
            "Subtract" => BinaryOperators.Difference,
            "Divide" => BinaryOperators.Ratio,
            _ => (a, b) => a
        };
        var legacy = MathHelper.ApplyBinaryOperation(values1, values2, legacyOp);
        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0, 1);
        var modern = expression != null ? TransformExpressionEvaluator.Evaluate(expression, [aligned1, aligned2]) : legacy;
        return (CompareResults(legacy, modern), legacy.Count, modern.Count, expression != null);
    }

    private static ParityResultSnapshot CompareResults(IReadOnlyList<double> legacy, IReadOnlyList<double> modern)
    {
        if (legacy.Count != modern.Count)
            return new ParityResultSnapshot { Passed = false, Error = $"Result count mismatch: legacy={legacy.Count}, new={modern.Count}" };

        const double epsilon = 1e-6;
        for (var i = 0; i < legacy.Count; i++)
        {
            if (double.IsNaN(legacy[i]) && double.IsNaN(modern[i]))
                continue;

            if (Math.Abs(legacy[i] - modern[i]) > epsilon)
                return new ParityResultSnapshot { Passed = false, Error = $"Value mismatch at index {i}: legacy={legacy[i]}, new={modern[i]}" };
        }

        return new ParityResultSnapshot { Passed = true, Message = "Transform parity validation passed" };
    }
}
