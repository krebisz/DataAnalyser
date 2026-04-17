using System.Windows.Controls;

namespace DataVisualiser.UI.Charts.Interfaces;

public interface ITransformLayoutCapabilities
{
    DataGridLength ResultGridColumnWidth { get; }
    bool UsesAutomaticChartWidth { get; }
    void UpdateAuxiliaryVisuals();
    void ResetAuxiliaryVisuals();
}
