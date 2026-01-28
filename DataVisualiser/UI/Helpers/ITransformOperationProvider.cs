using System.Collections.Generic;

namespace DataVisualiser.UI.Helpers;

public interface ITransformOperationProvider
{
    IReadOnlyList<(string Content, string Tag)> GetOperations();
}
