using System.Collections.Generic;

namespace DataVisualiser.UI.Controls;

public interface IChartControllerRegistry
{
    void Register(IChartController controller);
    IChartController Get(string key);
    IReadOnlyList<IChartController> All();
}
