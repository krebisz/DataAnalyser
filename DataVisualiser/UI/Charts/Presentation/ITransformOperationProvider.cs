using System.Collections.Generic;

namespace DataVisualiser.UI.Charts.Presentation;

public interface ITransformOperationProvider
{
    IReadOnlyList<(string Content, string Tag)> GetOperations();
}
