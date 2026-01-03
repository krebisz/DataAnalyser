using System.Windows.Controls;

namespace DataVisualiser.UI.SubtypeSelectors;

public class SubtypeControlPair
{
    public SubtypeControlPair(Label label, ComboBox combo)
    {
        Label = label;
        Combo = combo;
    }

    public Label    Label { get; }
    public ComboBox Combo { get; }
}