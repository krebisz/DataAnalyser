using System.Windows;
using System.Windows.Controls;

namespace DataVisualiser.UI.SubtypeSelectors;

public class SubtypeSelectorManager
{
    /// <summary>
    ///     List of dynamically-added ComboBoxes for additional subtypes.
    /// </summary>
    private readonly List<ComboBox> _dynamicCombos = new();

    private readonly List<SubtypeControlPair> _dynamicControls = new();
    private readonly Panel _parentPanel;

    public SubtypeSelectorManager(Panel parentPanel, ComboBox primaryCombo)
    {
        _parentPanel = parentPanel;
        PrimaryCombo = primaryCombo;

        // Hook the primary combo to our unified handler
        PrimaryCombo.SelectionChanged += (s, e) => SubtypeSelectionChanged?.Invoke(this, EventArgs.Empty);
    }


    /// <summary>
    ///     The primary static ComboBox (SubtypeCombo).
    ///     MainWindow will pass this reference into the manager.
    /// </summary>
    public ComboBox PrimaryCombo { get; }

    /// <summary>
    ///     Unified event fired when ANY subtype combo selection changes.
    /// </summary>
    public event EventHandler? SubtypeSelectionChanged;

    // ============================================================
    // Create a new dynamic subtype ComboBox
    // ============================================================
    public ComboBox AddSubtypeCombo(IEnumerable<string> subtypeList)
    {
        var index = _dynamicControls.Count + 2;

        var label = CreateSubtypeLabel(index);
        var combo = CreateSubtypeCombo(subtypeList);

        InsertControls(label, combo);

        _dynamicControls.Add(new SubtypeControlPair(label, combo));
        _dynamicCombos.Add(combo);

        return combo;
    }

    private static Label CreateSubtypeLabel(int index)
    {
        return new Label
        {
                Content = $"Metric Subtype {index}:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0)
        };
    }

    private ComboBox CreateSubtypeCombo(IEnumerable<string> subtypeList)
    {
        var combo = new ComboBox
        {
                Width = 250,
                Margin = new Thickness(5),
                IsEditable = false,
                IsEnabled = true
        };

        foreach (var subtype in subtypeList)
            combo.Items.Add(subtype);

        combo.SelectedIndex = 0;
        combo.SelectionChanged += (_, _) => SubtypeSelectionChanged?.Invoke(this, EventArgs.Empty);

        return combo;
    }

    private void InsertControls(Label label, ComboBox combo)
    {
        var (button, index) = FindAddSubtypeButton();

        if (button != null && index >= 0)
        {
            _parentPanel.Children.Insert(index, label);
            _parentPanel.Children.Insert(index + 1, combo);
        }
        else
        {
            _parentPanel.Children.Add(label);
            _parentPanel.Children.Add(combo);
        }
    }

    private(Button? Button, int Index) FindAddSubtypeButton()
    {
        for (var i = _parentPanel.Children.Count - 1; i >= 0; i--)
            if (_parentPanel.Children[i] is Button btn && btn.Content?.ToString() == "Add Subtype")
                return (btn, i);

        return (null, -1);
    }


    // ============================================================
    // Clear dynamic combos but keep the primary combo intact
    // ============================================================
    public void ClearDynamic()
    {
        foreach (var pair in _dynamicControls)
        {
            _parentPanel.Children.Remove(pair.Label);
            _parentPanel.Children.Remove(pair.Combo);
        }

        _dynamicControls.Clear();
        _dynamicCombos.Clear(); // Also clear _dynamicCombos
    }


    // ============================================================
    // Retrieve all active ComboBoxes (primary + dynamic)
    // ============================================================
    public IReadOnlyList<ComboBox> GetActiveCombos()
    {
        return new[]
                {
                        PrimaryCombo
                }.Concat(_dynamicControls.Select(p => p.Combo))
                 .ToList();
    }


    // ============================================================
    // Get selected subtype values
    // ============================================================
    public string? GetPrimarySubtype()
    {
        return PrimaryCombo.SelectedItem?.ToString();
    }

    public string? GetSecondarySubtype()
    {
        return _dynamicCombos.Count > 0 ? _dynamicCombos[0].SelectedItem?.ToString() : null;
    }

    public IEnumerable<string> GetAllSelectedSubtypes()
    {
        yield return GetPrimarySubtype() ?? string.Empty;

        foreach (var combo in _dynamicCombos)
            yield return combo.SelectedItem?.ToString() ?? string.Empty;
    }
}