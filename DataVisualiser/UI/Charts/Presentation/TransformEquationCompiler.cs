using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class TransformEquationCompiler
{
    public static TransformEquationCompileResult Compile(
        IReadOnlyList<TransformEquationTerm> terms,
        int inputCount)
    {
        if (terms == null || terms.Count == 0)
            return TransformEquationCompileResult.Invalid("Add at least one equation term before computing.");
        if (inputCount <= 0)
            return TransformEquationCompileResult.Invalid("At least one input is required.");

        var steps = new List<OperationChainStep>();
        var currentIndex = -1;
        var currentLabel = string.Empty;

        for (var index = 0; index < terms.Count; index++)
        {
            var term = terms[index];
            if (term.InputIndex < 0 || term.InputIndex >= inputCount)
                return TransformEquationCompileResult.Invalid($"Input {term.InputIndex + 1} is no longer available.");

            if (index == 0)
            {
                var first = CompileFirstTerm(term, inputCount, steps);
                if (!first.IsValid)
                    return TransformEquationCompileResult.Invalid(first.Error!);

                currentIndex = first.OutputIndex;
                currentLabel = first.OutputLabel;
                continue;
            }

            var next = CompileBinaryTerm(term, currentIndex, currentLabel, inputCount, steps);
            if (!next.IsValid)
                return TransformEquationCompileResult.Invalid(next.Error!);

            currentIndex = next.OutputIndex;
            currentLabel = next.OutputLabel;
        }

        if (steps.Count == 0)
            steps.Add(CreateStep(SeriesOperationRequest.Identity(currentIndex, "transform-equation::identity", currentLabel), "None"));

        return TransformEquationCompileResult.Valid(steps, currentLabel);
    }

    public static string BuildExpression(IReadOnlyList<TransformEquationTerm> terms)
    {
        if (terms == null || terms.Count == 0)
            return string.Empty;

        var expression = FormatFirstTerm(terms[0]);
        for (var index = 1; index < terms.Count; index++)
        {
            var term = terms[index];
            var label = ResolveExpressionLabel(term);
            expression = term.OperationTag switch
            {
                "Add" => $"({expression}) + {label}",
                "Subtract" => $"({expression}) - {label}",
                "Divide" => $"({expression}) / {label}",
                _ => $"{expression} ? {label}"
            };
        }

        return expression;
    }

    public static bool IsSupportedEquationOperation(string? operationTag) =>
        TransformSeriesOperationRequestMapper.IsPassThrough(operationTag) ||
        string.Equals(operationTag, "Log", StringComparison.Ordinal) ||
        string.Equals(operationTag, "Sqrt", StringComparison.Ordinal) ||
        string.Equals(operationTag, "Add", StringComparison.Ordinal) ||
        string.Equals(operationTag, "Subtract", StringComparison.Ordinal) ||
        string.Equals(operationTag, "Divide", StringComparison.Ordinal);

    private static TransformTermCompileResult CompileFirstTerm(
        TransformEquationTerm term,
        int inputCount,
        List<OperationChainStep> steps)
    {
        var input = term.InputIndex;
        if (TransformSeriesOperationRequestMapper.IsPassThrough(term.OperationTag) || IsBinary(term.OperationTag))
            return TransformTermCompileResult.Valid(input, term.InputLabel);

        var request = term.OperationTag switch
        {
            "Log" => SeriesOperationRequest.Logarithm(input, $"transform-equation::log::{input}", $"Log({term.InputLabel})"),
            "Sqrt" => SeriesOperationRequest.SquareRoot(input, $"transform-equation::sqrt::{input}", $"Sqrt({term.InputLabel})"),
            _ => null
        };

        if (request == null)
            return TransformTermCompileResult.Invalid($"Operation '{term.OperationLabel}' is not valid in the equation builder.");

        steps.Add(CreateStep(request, term.OperationTag));
        return TransformTermCompileResult.Valid(inputCount + steps.Count - 1, request.Label);
    }

    private static TransformTermCompileResult CompileBinaryTerm(
        TransformEquationTerm term,
        int currentIndex,
        string currentLabel,
        int inputCount,
        List<OperationChainStep> steps)
    {
        var request = term.OperationTag switch
        {
            "Add" => SeriesOperationRequest.Sum([currentIndex, term.InputIndex], $"{currentLabel} + {term.InputLabel}"),
            "Subtract" => SeriesOperationRequest.Difference(currentIndex, term.InputIndex, $"{currentLabel} - {term.InputLabel}"),
            "Divide" => SeriesOperationRequest.Ratio(currentIndex, term.InputIndex, $"{currentLabel} / {term.InputLabel}"),
            _ => null
        };

        if (request == null)
            return TransformTermCompileResult.Invalid("After the first term, add a binary operation: Add, Subtract, or Divide.");

        steps.Add(CreateStep(request, term.OperationTag));
        return TransformTermCompileResult.Valid(inputCount + steps.Count - 1, request.Label);
    }

    private static OperationChainStep CreateStep(SeriesOperationRequest request, string? operationTag) =>
        new(
            request,
            reversible: request.Kind == SeriesOperationKind.Identity,
            SeriesOperationRules.DefaultLossiness(request.Kind),
            new Dictionary<string, string>
            {
                ["Source"] = "TransformEquation",
                ["OperationTag"] = operationTag ?? string.Empty
            });

    private static string FormatFirstTerm(TransformEquationTerm term) =>
        term.OperationTag switch
        {
            "Log" => $"Log({ResolveExpressionLabel(term)})",
            "Sqrt" => $"Sqrt({ResolveExpressionLabel(term)})",
            _ => ResolveExpressionLabel(term)
        };

    private static string ResolveExpressionLabel(TransformEquationTerm term) =>
        string.IsNullOrWhiteSpace(term.ExpressionLabel)
            ? term.InputLabel
            : term.ExpressionLabel;

    private static bool IsBinary(string? operationTag) =>
        string.Equals(operationTag, "Add", StringComparison.Ordinal) ||
        string.Equals(operationTag, "Subtract", StringComparison.Ordinal) ||
        string.Equals(operationTag, "Divide", StringComparison.Ordinal);
}

internal sealed record TransformEquationTerm(
    string OperationTag,
    string OperationLabel,
    int InputIndex,
    string InputLabel,
    string? ExpressionLabel = null);

internal sealed record TransformEquationCompileResult(
    bool IsValid,
    IReadOnlyList<OperationChainStep> Steps,
    string Title,
    string? Error)
{
    public static TransformEquationCompileResult Valid(IReadOnlyList<OperationChainStep> steps, string title) =>
        new(true, steps, title, null);

    public static TransformEquationCompileResult Invalid(string error) =>
        new(false, Array.Empty<OperationChainStep>(), string.Empty, error);
}

internal sealed record TransformTermCompileResult(
    bool IsValid,
    int OutputIndex,
    string OutputLabel,
    string? Error)
{
    public static TransformTermCompileResult Valid(int outputIndex, string outputLabel) =>
        new(true, outputIndex, outputLabel, null);

    public static TransformTermCompileResult Invalid(string error) =>
        new(false, -1, string.Empty, error);
}
