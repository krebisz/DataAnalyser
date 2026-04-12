using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Transforms;
using DataVisualiser.Core.Transforms.Expressions;
using DataVisualiser.Core.Transforms.Operations;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost;

internal sealed class EvidenceTransformParityEvaluator
{
    private readonly Func<string?> _getSelectedTransformOperation;
    private readonly MetricSelectionService _metricSelectionService;

    internal EvidenceTransformParityEvaluator(
        MetricSelectionService metricSelectionService,
        Func<string?> getSelectedTransformOperation)
    {
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _getSelectedTransformOperation = getSelectedTransformOperation ?? throw new ArgumentNullException(nameof(getSelectedTransformOperation));
    }

    internal async Task<TransformParitySnapshot> BuildAsync(ChartState chartState, MetricState metricState, ChartDataContext? ctx)
    {
        if (ctx == null)
            return new TransformParitySnapshot { Status = "Unavailable", Reason = "No chart context available" };

        var operation = _getSelectedTransformOperation();
        if (string.IsNullOrWhiteSpace(operation))
            return new TransformParitySnapshot { Status = "Unavailable", Reason = "No transform operation selected" };

        var (primarySelection, secondarySelection) = ResolveTransformSelections(chartState, ctx);
        var primaryData = await ResolveTransformParityDataAsync(metricState, ctx, primarySelection);
        if (primaryData == null || primaryData.Count == 0)
            return new TransformParitySnapshot { Status = "Unavailable", Reason = "No primary data available for transform" };

        var isUnary = IsUnaryTransform(operation);
        IReadOnlyList<MetricData>? secondaryData = null;
        if (!isUnary)
        {
            secondaryData = await ResolveTransformParityDataAsync(metricState, ctx, secondarySelection);
            if (secondaryData == null || secondaryData.Count == 0)
                return new TransformParitySnapshot { Status = "Unavailable", Reason = "No secondary data available for binary transform" };
        }

        var result = isUnary ? ComputeUnaryTransformParity(primaryData, operation) : ComputeBinaryTransformParity(primaryData, secondaryData!, operation);
        return new TransformParitySnapshot
        {
            Status = "Completed",
            Operation = operation,
            IsUnary = isUnary,
            ExpressionAvailable = result.ExpressionAvailable,
            LegacySamples = result.LegacySamples,
            NewSamples = result.NewSamples,
            Result = result.Result
        };
    }

    private static bool IsUnaryTransform(string operation)
    {
        return string.Equals(operation, "Log", StringComparison.OrdinalIgnoreCase) || string.Equals(operation, "Sqrt", StringComparison.OrdinalIgnoreCase);
    }

    private static (MetricSeriesSelection? Primary, MetricSeriesSelection? Secondary) ResolveTransformSelections(ChartState chartState, ChartDataContext ctx)
    {
        var primary = chartState.SelectedTransformPrimarySeries;
        var secondary = chartState.SelectedTransformSecondarySeries;
        if (primary != null || secondary != null)
            return (primary, secondary);

        var primaryMetricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        var primarySelection = string.IsNullOrWhiteSpace(primaryMetricType) ? null : new MetricSeriesSelection(primaryMetricType, ctx.PrimarySubtype);
        MetricSeriesSelection? secondarySelection = null;
        if (!string.IsNullOrWhiteSpace(ctx.SecondaryMetricType))
            secondarySelection = new MetricSeriesSelection(ctx.SecondaryMetricType, ctx.SecondarySubtype);

        return (primarySelection, secondarySelection);
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveTransformParityDataAsync(MetricState? metricState, ChartDataContext ctx, MetricSeriesSelection? selection)
    {
        if (selection == null)
            return null;

        if (ctx.Data1 != null && EvidenceDiagnosticsBuilder.IsSameSelection(selection, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.Data1;

        if (ctx.Data2 != null && EvidenceDiagnosticsBuilder.IsSameSelection(selection, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.Data2;

        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return ctx.Data1;

        var tableName = metricState?.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, ctx.From, ctx.To, tableName);
        return primaryData.ToList();
    }

    private static (ParityResultSnapshot Result, int LegacySamples, int NewSamples, bool ExpressionAvailable) ComputeUnaryTransformParity(IReadOnlyList<MetricData> data, string operation)
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
        return (CompareTransformResults(legacy, modern), legacy.Count, modern.Count, expression != null);
    }

    private static (ParityResultSnapshot Result, int LegacySamples, int NewSamples, bool ExpressionAvailable) ComputeBinaryTransformParity(IReadOnlyList<MetricData> data1, IReadOnlyList<MetricData> data2, string operation)
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
        return (CompareTransformResults(legacy, modern), legacy.Count, modern.Count, expression != null);
    }

    private static ParityResultSnapshot CompareTransformResults(IReadOnlyList<double> legacy, IReadOnlyList<double> modern)
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
