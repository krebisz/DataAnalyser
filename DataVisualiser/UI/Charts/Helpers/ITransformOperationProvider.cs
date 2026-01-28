using System.Collections.Generic;

namespace DataVisualiser.UI.Charts.Helpers;

public interface ITransformOperationProvider
{
    IReadOnlyList<(string Content, string Tag)> GetOperations();
}
