using System.Windows;
using System.Windows.Controls;


namespace DataVisualiser
{
    public class SubtypeComboBoxManager
    {
        private readonly StackPanel _panel;
        private readonly List<ComboBox> _comboBoxes = new();

        public IReadOnlyList<ComboBox> ComboBoxes => _comboBoxes;

        public SubtypeComboBoxManager(StackPanel panel)
        {
            _panel = panel;
            InitializeExistingComboBoxes();
        }

        /// <summary>
        /// Load any ComboBoxes already present in XAML.
        /// </summary>
        private void InitializeExistingComboBoxes()
        {
            var existing = _panel.Children.OfType<ComboBox>().ToList();
            foreach (var cb in existing)
                _comboBoxes.Add(cb);
        }

        /// <summary>
        /// Creates a new ComboBox, adds it to the panel, and tracks it.
        /// </summary>
        public ComboBox AddSubtypeComboBox(string? labelPrefix = null)
        {
            string label = labelPrefix ?? $"Metric Subtype {_comboBoxes.Count + 1}:";

            // Insert a Label before each ComboBox
            var lbl = new Label
            {
                Content = label,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0)
            };

            var combo = new ComboBox
            {
                Width = 250,
                Margin = new Thickness(5),
                IsEditable = false,
                IsEnabled = true,
                Visibility = Visibility.Visible
            };

            _panel.Children.Add(lbl);
            _panel.Children.Add(combo);

            _comboBoxes.Add(combo);
            return combo;
        }

        /// <summary>
        /// Remove the last ComboBox from the panel.
        /// </summary>
        public bool RemoveLastComboBox()
        {
            if (_comboBoxes.Count == 0)
                return false;

            var last = _comboBoxes.Last();
            int indexInPanel = _panel.Children.IndexOf(last);

            // Remove the ComboBox and the Label preceding it
            if (indexInPanel > 0 && _panel.Children[indexInPanel - 1] is Label)
                _panel.Children.RemoveAt(indexInPanel - 1);

            _panel.Children.Remove(last);

            _comboBoxes.Remove(last);
            return true;
        }

        /// <summary>
        /// Hide all ComboBoxes.
        /// </summary>
        public void HideAll()
        {
            foreach (var cb in _comboBoxes)
                cb.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Show all ComboBoxes.
        /// </summary>
        public void ShowAll()
        {
            foreach (var cb in _comboBoxes)
                cb.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Enable/Disable all ComboBoxes.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            foreach (var cb in _comboBoxes)
                cb.IsEnabled = enabled;
        }

        /// <summary>
        /// Get only ComboBoxes that are visible and enabled.
        /// </summary>
        public List<ComboBox> GetActiveComboBoxes()
        {
            return _comboBoxes
                .Where(cb => cb.Visibility == Visibility.Visible && cb.IsEnabled)
                .ToList();
        }

        /// <summary>
        /// Remove ALL dynamically added ComboBoxes.
        /// Does not touch the initial XAML ones unless asked.
        /// </summary>
        public void ClearDynamic(int keepFirstCount = 1)
        {
            while (_comboBoxes.Count > keepFirstCount)
                RemoveLastComboBox();
        }
    }

}
