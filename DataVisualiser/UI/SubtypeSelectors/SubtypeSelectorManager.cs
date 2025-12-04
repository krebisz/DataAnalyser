using System.Windows.Controls;

namespace DataVisualiser.UI.SubtypeSelectors
{
    public class SubtypeSelectorManager
    {
        private readonly Panel _parentPanel;
        private readonly List<SubtypeControlPair> _dynamicControls = new();


        /// <summary>
        /// The primary static ComboBox (SubtypeCombo).
        /// MainWindow will pass this reference into the manager.
        /// </summary>
        public ComboBox PrimaryCombo { get; }

        /// <summary>
        /// List of dynamically-added ComboBoxes for additional subtypes.
        /// </summary>
        private readonly List<ComboBox> _dynamicCombos = new();

        /// <summary>
        /// Unified event fired when ANY subtype combo selection changes.
        /// </summary>
        public event EventHandler? SubtypeSelectionChanged;

        public SubtypeSelectorManager(Panel parentPanel, ComboBox primaryCombo)
        {
            _parentPanel = parentPanel;
            PrimaryCombo = primaryCombo;

            // Hook the primary combo to our unified handler
            PrimaryCombo.SelectionChanged += (s, e) =>
                SubtypeSelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        // ============================================================
        // Create a new dynamic subtype ComboBox
        // ============================================================
        public ComboBox AddSubtypeCombo(IEnumerable<string> subtypeList)
        {
            int index = _dynamicControls.Count + 2;   // 2nd, 3rd, 4th subtype…

            var label = new Label
            {
                Content = $"Metric Subtype {index}:",
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new System.Windows.Thickness(5, 0, 0, 0)
            };

            var combo = new ComboBox
            {
                Width = 250,
                Margin = new System.Windows.Thickness(5),
                IsEditable = false,
                IsEnabled = true
            };

            foreach (var subtype in subtypeList)
                combo.Items.Add(subtype);

            combo.SelectedIndex = 0;

            combo.SelectionChanged += (s, e) =>
                SubtypeSelectionChanged?.Invoke(this, EventArgs.Empty);

            // ADD BOTH TO THE PANEL IN ORDER
            _parentPanel.Children.Add(label);
            _parentPanel.Children.Add(combo);

            _dynamicControls.Add(new SubtypeControlPair(label, combo));
            _dynamicCombos.Add(combo); // Also add to _dynamicCombos for GetSecondarySubtype()

            return combo;
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
            var list = new List<ComboBox> { PrimaryCombo };
            list.AddRange(_dynamicControls.Select(p => p.Combo));
            return list;
        }


        // ============================================================
        // Get selected subtype values
        // ============================================================
        public string? GetPrimarySubtype() =>
            PrimaryCombo.SelectedItem?.ToString();

        public string? GetSecondarySubtype() =>
            _dynamicCombos.Count > 0 ? _dynamicCombos[0].SelectedItem?.ToString() : null;

        public IEnumerable<string> GetAllSelectedSubtypes()
        {
            yield return GetPrimarySubtype() ?? string.Empty;

            foreach (var combo in _dynamicCombos)
                yield return combo.SelectedItem?.ToString() ?? string.Empty;
        }
    }

    public class SubtypeControlPair
    {
        public Label Label { get; }
        public ComboBox Combo { get; }

        public SubtypeControlPair(Label label, ComboBox combo)
        {
            Label = label;
            Combo = combo;
        }
    }

}
