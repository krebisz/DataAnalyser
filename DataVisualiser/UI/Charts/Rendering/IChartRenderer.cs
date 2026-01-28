using System.Threading;
using System.Threading.Tasks;

namespace DataVisualiser.UI.Charts.Rendering;

public interface IChartRenderer
{
    Task ApplyAsync(IChartSurface surface, UiChartRenderModel model, CancellationToken cancellationToken = default);
}
