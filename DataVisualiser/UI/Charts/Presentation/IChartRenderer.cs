using System.Threading;
using System.Threading.Tasks;

namespace DataVisualiser.UI.Charts.Presentation;

public interface IChartRenderer
{
    Task ApplyAsync(IChartSurface surface, UiChartRenderModel model, CancellationToken cancellationToken = default);
}
