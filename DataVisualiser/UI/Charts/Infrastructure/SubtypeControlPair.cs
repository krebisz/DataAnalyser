using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Adapters;
using System.Windows.Controls;

namespace DataVisualiser.UI.Charts.Infrastructure;

public class SubtypeControlPair
{
    public SubtypeControlPair(Label label, ComboBox combo)
    {
        Label = label;
        Combo = combo;
    }

    public Label Label { get; }
    public ComboBox Combo { get; }
}
