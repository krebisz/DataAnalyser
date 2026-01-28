using System.Collections.Generic;

namespace DataVisualiser.UI.Helpers;

public sealed class DefaultTransformOperationProvider : ITransformOperationProvider
{
    private static readonly IReadOnlyList<(string Content, string Tag)> Operations = new[]
    {
            ("Logarithm", "Log"),
            ("Square Root", "Sqrt"),
            ("Add (+)", "Add"),
            ("Subtract (-)", "Subtract"),
            ("Divide (/)", "Divide")
    };

    public IReadOnlyList<(string Content, string Tag)> GetOperations() => Operations;
}
