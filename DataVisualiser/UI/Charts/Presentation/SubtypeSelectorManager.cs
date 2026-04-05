using DataVisualiser.UI.Charts.Interfaces;
using System.Windows;
using System.Windows.Controls;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Charts.Presentation;

public class SubtypeSelectorManager
{
    /// <summary>
    ///     List of dynamically-added ComboBoxes for additional subtypes.
    /// </summary>
    private readonly List<ComboBox> _dynamicCombos = new();

    private readonly List<SubtypeControlPair> _dynamicControls = new();
    private readonly Dictionary<ComboBox, MetricNameOption?> _metricTypesByCombo = new();
    private readonly Panel _parentPanel;
    private int _selectionChangedSuppressionDepth;

    public SubtypeSelectorManager(Panel parentPanel, ComboBox primaryCombo)
    {
        _parentPanel = parentPanel;
        PrimaryCombo = primaryCombo;
        _metricTypesByCombo[primaryCombo] = null;

        // Hook the primary combo to our unified handler
        PrimaryCombo.SelectionChanged += (_, _) => RaiseSubtypeSelectionChanged();
    }


    /// <summary>
    ///     The primary static ComboBox (SubtypeCombo).
    ///     MainWindow will pass this reference into the manager.
    /// </summary>
    public ComboBox PrimaryCombo { get; }

    public bool HasDynamicCombos => _dynamicControls.Count > 0;

    /// <summary>
    ///     Unified event fired when ANY subtype combo selection changes.
    /// </summary>
    public event EventHandler? SubtypeSelectionChanged;

    public IDisposable SuppressSelectionChanged()
    {
        _selectionChangedSuppressionDepth++;
        return new SelectionChangedSuppressionScope(this);
    }

    // ============================================================
    // Create a new dynamic subtype ComboBox
    // ============================================================
    public ComboBox AddSubtypeCombo(IEnumerable<MetricNameOption> subtypeList, MetricNameOption? metricType)
    {
        using var _ = SuppressSelectionChanged();
        var index = _dynamicControls.Count + 2;

        var label = CreateSubtypeLabel(index);
        var combo = CreateSubtypeCombo(subtypeList);

        InsertControls(label, combo);

        _dynamicControls.Add(new SubtypeControlPair(label, combo));
        _dynamicCombos.Add(combo);
        _metricTypesByCombo[combo] = metricType;

        return combo;
    }

    public void SetPrimaryMetricType(MetricNameOption? metricType)
    {
        _metricTypesByCombo[PrimaryCombo] = metricType;
    }

    public void ReplacePrimaryItems(IEnumerable<MetricNameOption> subtypeList, MetricNameOption? metricType, bool preserveSelection)
    {
        using var _ = SuppressSelectionChanged();
        var previousSelection = GetSelectedValue(PrimaryCombo);
        var subtypeOptions = subtypeList.ToList();

        PrimaryCombo.Items.Clear();
        foreach (var subtype in subtypeOptions)
            PrimaryCombo.Items.Add(subtype);

        PrimaryCombo.IsEnabled = subtypeOptions.Count > 0;
        SetPrimaryMetricType(metricType);

        if (preserveSelection && !string.IsNullOrWhiteSpace(previousSelection))
        {
            var preserved = PrimaryCombo.Items
                .OfType<MetricNameOption>()
                .FirstOrDefault(item => string.Equals(item.Value, previousSelection, StringComparison.OrdinalIgnoreCase));

            if (preserved != null)
            {
                PrimaryCombo.SelectedItem = preserved;
                return;
            }
        }

        SelectFirstItem(PrimaryCombo);
    }

    public void ClearAllSubtypeControls()
    {
        using var _ = SuppressSelectionChanged();
        ClearDynamic();
        PrimaryCombo.Items.Clear();
        PrimaryCombo.SelectedItem = null;
        PrimaryCombo.IsEnabled = false;
    }

    public void EnsurePrimarySelection()
    {
        using var _ = SuppressSelectionChanged();
        EnsureSelectionMaterialized(PrimaryCombo);
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

    private ComboBox CreateSubtypeCombo(IEnumerable<MetricNameOption> subtypeList)
    {
        var combo = new ComboBox
        {
                Width = 250,
                Margin = new Thickness(5),
                IsEditable = false,
                IsEnabled = true,
                DisplayMemberPath = "Display",
                SelectedValuePath = "Value"
        };

        foreach (var subtype in subtypeList)
            combo.Items.Add(subtype);

        SelectFirstItem(combo);
        combo.SelectionChanged += (_, _) => RaiseSubtypeSelectionChanged();

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
        foreach (var combo in _dynamicCombos)
            _metricTypesByCombo.Remove(combo);

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

    public IReadOnlyList<MetricSeriesSelection> GetSelectedSeries()
    {
        var selections = new List<MetricSeriesSelection>();
        foreach (var combo in GetActiveCombos())
        {
            EnsureSelectionMaterialized(combo);

            if (combo.SelectedItem == null)
                continue;

            var metricType = GetMetricTypeForCombo(combo);
            if (metricType == null || string.IsNullOrWhiteSpace(metricType.Value))
                continue;

            var subtypeOption = combo.SelectedItem as MetricNameOption;
            var subtypeValue = subtypeOption?.Value ?? combo.SelectedValue?.ToString() ?? combo.SelectedItem?.ToString();
            var selection = new MetricSeriesSelection(metricType.Value, subtypeValue, metricType.Display, subtypeOption?.Display);
            if (selection.QuerySubtype == null)
                continue;

            selections.Add(selection);
        }

        return selections;
    }

    public bool UpdateLastDynamicComboItems(IEnumerable<MetricNameOption> subtypeList)
    {
        if (_dynamicControls.Count == 0)
            return false;

        using var _ = SuppressSelectionChanged();
        var combo = _dynamicControls[^1].Combo;
        var previousSelection = (combo.SelectedItem as MetricNameOption)?.Value ?? combo.SelectedValue?.ToString() ?? combo.SelectedItem?.ToString();

        combo.Items.Clear();
        foreach (var subtype in subtypeList)
            combo.Items.Add(subtype);

        if (combo.Items.Count == 0)
        {
            combo.SelectedItem = null;
            combo.IsEnabled = false;
            return true;
        }

        combo.IsEnabled = true;

        if (!string.IsNullOrWhiteSpace(previousSelection) && combo.Items.OfType<MetricNameOption>().Any(item => string.Equals(item.Value, previousSelection, StringComparison.OrdinalIgnoreCase)))
        {
            combo.SelectedItem = combo.Items.OfType<MetricNameOption>().First(item => string.Equals(item.Value, previousSelection, StringComparison.OrdinalIgnoreCase));
            return true;
        }

        combo.SelectedIndex = 0;
        return true;
    }

    public bool UpdateLastDynamicComboItems(IEnumerable<MetricNameOption> subtypeList, MetricNameOption? metricType)
    {
        if (!UpdateLastDynamicComboItems(subtypeList))
            return false;

        _metricTypesByCombo[_dynamicControls[^1].Combo] = metricType;
        return true;
    }

    public MetricNameOption? GetMetricTypeForCombo(ComboBox combo)
    {
        return _metricTypesByCombo.TryGetValue(combo, out var metricType) ? metricType : null;
    }


    // ============================================================
    // Get selected subtype values
    // ============================================================
    public string? GetPrimarySubtype()
    {
        EnsureSelectionMaterialized(PrimaryCombo);
        return (PrimaryCombo.SelectedItem as MetricNameOption)?.Value ?? PrimaryCombo.SelectedValue?.ToString();
    }

    public string? GetSecondarySubtype()
    {
        return _dynamicCombos.Count > 0 ? (_dynamicCombos[0].SelectedItem as MetricNameOption)?.Value ?? _dynamicCombos[0].SelectedValue?.ToString() : null;
    }

    public IEnumerable<string> GetAllSelectedSubtypes()
    {
        yield return GetPrimarySubtype() ?? string.Empty;

        foreach (var combo in _dynamicCombos)
            yield return (combo.SelectedItem as MetricNameOption)?.Value ?? combo.SelectedValue?.ToString() ?? string.Empty;
    }

    private static string? GetSelectedValue(ComboBox combo)
    {
        return (combo.SelectedItem as MetricNameOption)?.Value ?? combo.SelectedValue?.ToString() ?? combo.SelectedItem?.ToString();
    }

    private static void SelectFirstItem(ComboBox combo)
    {
        if (combo.Items.Count == 0)
        {
            combo.SelectedItem = null;
            return;
        }

        combo.SelectedItem = combo.Items[0];
    }

    private static void EnsureSelectionMaterialized(ComboBox combo)
    {
        if (combo.SelectedItem != null || combo.Items.Count == 0)
            return;

        if (combo.SelectedIndex >= 0 && combo.SelectedIndex < combo.Items.Count)
        {
            combo.SelectedItem = combo.Items[combo.SelectedIndex];
            return;
        }

        combo.SelectedItem = combo.Items[0];
    }

    private void RaiseSubtypeSelectionChanged()
    {
        if (_selectionChangedSuppressionDepth > 0)
            return;

        SubtypeSelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private sealed class SelectionChangedSuppressionScope : IDisposable
    {
        private readonly SubtypeSelectorManager _owner;
        private bool _disposed;

        public SelectionChangedSuppressionScope(SubtypeSelectorManager owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            if (_owner._selectionChangedSuppressionDepth > 0)
                _owner._selectionChangedSuppressionDepth--;
        }
    }

    private sealed class SubtypeControlPair
    {
        public SubtypeControlPair(Label label, ComboBox combo)
        {
            Label = label;
            Combo = combo;
        }

        public Label Label { get; }
        public ComboBox Combo { get; }
    }
}
