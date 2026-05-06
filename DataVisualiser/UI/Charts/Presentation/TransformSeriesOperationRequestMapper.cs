using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class TransformSeriesOperationRequestMapper
{
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
        string primaryLabel,
        string? secondaryLabel,
        out OperationChainStep? step)
    {
        step = null;

        if (!TryCreate(operationTag, primaryLabel, secondaryLabel, out var request) || request == null)
            return false;

        var metadata = new Dictionary<string, string>
        {
            ["Source"] = "TransformTab",
            ["OperationTag"] = operationTag ?? string.Empty
        };

        step = new OperationChainStep(
            request,
            reversible: request.Kind == SeriesOperationKind.Identity,
            SeriesOperationRules.DefaultLossiness(request.Kind),
            metadata);
        return true;
    }
}
