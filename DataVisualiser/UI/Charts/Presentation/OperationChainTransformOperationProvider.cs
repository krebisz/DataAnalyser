namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class OperationChainTransformOperationProvider : ITransformOperationProvider
{
    private static readonly IReadOnlyList<(string Content, string Tag)> Operations =
    [
        ("None", "None"),
        ("Logarithm", "Log"),
        ("Square Root", "Sqrt"),
        ("Add (+)", "Add"),
        ("Subtract (-)", "Subtract"),
        ("Divide (/)", "Divide"),
        ("Ternary Sum (+ +)", "Sum3"),
        ("Correlation", "Correlation"),
        ("Ternary Sum Correlation", "Sum3Correlation")
    ];

    public IReadOnlyList<(string Content, string Tag)> GetOperations() => Operations;
}
