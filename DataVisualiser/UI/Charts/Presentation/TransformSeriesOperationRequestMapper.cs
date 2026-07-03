using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class TransformSeriesOperationRequestMapper
{
    public static bool IsPassThrough(string? operationTag) =>
        string.IsNullOrWhiteSpace(operationTag) ||
        string.Equals(operationTag, "None", StringComparison.OrdinalIgnoreCase);

    public static bool TryCreate(
        string? operationTag,
        string primaryLabel,
        string? secondaryLabel,
        out SeriesOperationRequest? request)
    {
        request = null;
        var primary = string.IsNullOrWhiteSpace(primaryLabel) ? "Primary" : primaryLabel;
        var secondary = string.IsNullOrWhiteSpace(secondaryLabel) ? "Secondary" : secondaryLabel;

        if (string.IsNullOrWhiteSpace(operationTag))
        {
            request = SeriesOperationRequest.Identity(0, "transform::identity", primary);
            return true;
        }

        request = operationTag switch
        {
            "Log" => SeriesOperationRequest.Logarithm(0, "transform::log", $"Log({primary})"),
            "Sqrt" => SeriesOperationRequest.SquareRoot(0, "transform::sqrt", $"Sqrt({primary})"),
            "Add" => SeriesOperationRequest.Sum([0, 1], $"{primary} + {secondary}"),
            "Subtract" => SeriesOperationRequest.Difference(0, 1, $"{primary} - {secondary}"),
            "Divide" => SeriesOperationRequest.Ratio(0, 1, $"{primary} / {secondary}"),
            _ => null
        };

        return request != null;
    }

    public static bool TryCreateOperationChainStep(
        string? operationTag,
        IReadOnlyList<MetricSeriesRequest> series,
        out OperationChainStep? step)
    {
        step = null;
        if (string.IsNullOrWhiteSpace(operationTag) || series.Count == 0)
            return false;

        var first = ResolveLabel(series[0]);
        var second = series.Count > 1 ? ResolveLabel(series[1]) : "Input 2";
        var third = series.Count > 2 ? ResolveLabel(series[2]) : "Input 3";
        var request = operationTag switch
        {
            "Correlation" when series.Count >= 2 => SeriesOperationRequest.Identity(0, "operation-chain::correlation", $"{first} ~ {second}"),
            "Log" => SeriesOperationRequest.Logarithm(0, "operation-chain::log", $"Log({first})"),
            "Sqrt" => SeriesOperationRequest.SquareRoot(0, "operation-chain::sqrt", $"Sqrt({first})"),
            "Add" when series.Count >= 2 => SeriesOperationRequest.Sum([0, 1], $"{first} + {second}"),
            "Subtract" when series.Count >= 2 => SeriesOperationRequest.Difference(0, 1, $"{first} - {second}"),
            "Divide" when series.Count >= 2 => SeriesOperationRequest.Ratio(0, 1, $"{first} / {second}"),
            "Sum3" when series.Count >= 3 => SeriesOperationRequest.Sum([0, 1, 2], $"{first} + {second} + {third}"),
            "Sum3Correlation" when series.Count >= 4 => SeriesOperationRequest.Sum([0, 1, 2], $"{first} + {second} + {third}"),
            _ => null
        };

        if (request == null)
            return false;

        step = CreateStep(request, operationTag, "OperationChainWorkbench");
        return true;
    }

    public static bool TryCreateOperationChainSteps(
        string? operationTag,
        IReadOnlyList<MetricSeriesRequest> series,
        out IReadOnlyList<OperationChainStep> steps,
        out string title)
    {
        steps = [];
        title = string.Empty;

        if (string.IsNullOrWhiteSpace(operationTag) || series.Count == 0)
            return false;

        if (TryResolveNormalizationMode(operationTag, out var mode))
        {
            title = ResolveNormalizationTitle(mode);
            var normalizationTitle = title;
            var references = Enumerable.Range(0, series.Count).ToArray();
            steps = series
                .Select((request, index) => CreateStep(
                    SeriesOperationRequest.Normalize(
                        index,
                        $"operation-chain::normalize::{mode}::{index}::{request.SignatureToken}",
                        $"{request.DisplayName} ({normalizationTitle})",
                        mode,
                        references),
                    operationTag,
                    "OperationChainWorkbench"))
                .ToArray();
            return true;
        }

        if (!TryCreateOperationChainStep(operationTag, series, out var step) || step == null)
            return false;

        steps = [step];
        title = step.Operation.Label;
        return true;
    }

    public static bool TryCreateOperationChainStep(
        string? operationTag,
        string primaryLabel,
        string? secondaryLabel,
        out OperationChainStep? step)
    {
        step = null;

        if (!TryCreate(operationTag, primaryLabel, secondaryLabel, out var request) || request == null)
            return false;

        step = CreateStep(request, operationTag, "TransformTab");
        return true;
    }

    private static OperationChainStep CreateStep(
        SeriesOperationRequest request,
        string? operationTag,
        string source)
    {
        return new OperationChainStep(
            request,
            reversible: request.Kind == SeriesOperationKind.Identity,
            SeriesOperationRules.DefaultLossiness(request.Kind),
            new Dictionary<string, string>
            {
                ["Source"] = source,
                ["OperationTag"] = operationTag ?? string.Empty
            });
    }

    public static bool TryResolveNormalizationMode(
        string? operationTag,
        out NormalizationMode mode)
    {
        mode = operationTag switch
        {
            "NormalizeZeroToOne" => NormalizationMode.ZeroToOne,
            "NormalizePercentageOfMax" => NormalizationMode.PercentageOfMax,
            "NormalizeRelativeToMax" => NormalizationMode.RelativeToMax,
            _ => default
        };

        return operationTag is "NormalizeZeroToOne" or "NormalizePercentageOfMax" or "NormalizeRelativeToMax";
    }

    public static string ResolveNormalizationTitle(NormalizationMode mode)
    {
        return mode switch
        {
            NormalizationMode.ZeroToOne => "Zero-To-One",
            NormalizationMode.PercentageOfMax => "% of Max",
            NormalizationMode.RelativeToMax => "Relative to Max",
            _ => "Normalized"
        };
    }

    private static string ResolveLabel(MetricSeriesRequest request) =>
        string.IsNullOrWhiteSpace(request.DisplaySubtype)
            ? request.DisplayName
            : request.DisplaySubtype;
}
