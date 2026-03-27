using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Rendering;

public sealed class PieFacetLegendToggleManager
{
    private readonly IReadOnlyList<PieChart> _charts;

    public PieFacetLegendToggleManager(IEnumerable<PieChart> charts)
    {
        if (charts == null)
            throw new ArgumentNullException(nameof(charts));

        _charts = charts.Where(chart => chart != null).ToList();
        RebuildItems();
    }

    public ObservableCollection<LegendItem> Items { get; } = new();

    public void AttachItemsControl(System.Windows.Controls.ItemsControl itemsControl)
    {
        if (itemsControl == null)
            throw new ArgumentNullException(nameof(itemsControl));

        itemsControl.ItemsSource = Items;
    }

    public static void HandleToggle(object sender)
    {
        if (sender is not ToggleButton toggleButton || toggleButton.DataContext is not LegendItem item)
            return;

        item.IsVisible = toggleButton.IsChecked == true;
        var visibility = item.IsVisible ? Visibility.Visible : Visibility.Collapsed;

        foreach (var series in item.Series)
            series.Visibility = visibility;

        foreach (var chart in item.Charts)
            chart.Update(true, true);
    }

    private void RebuildItems()
    {
        Items.Clear();

        var groupedSeries = _charts
            .SelectMany(chart => chart.Series.OfType<PieSeries>())
            .GroupBy(series => string.IsNullOrWhiteSpace(series.Title) ? "Series" : series.Title, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var group in groupedSeries)
        {
            var seriesList = group.ToList();
            var representative = seriesList[0];
            var brush = representative.Fill ?? representative.Stroke ?? Brushes.Gray;

            Items.Add(new LegendItem(
                group.Key,
                brush,
                seriesList,
                _charts,
                seriesList.Any(series => series.Visibility != Visibility.Collapsed)));
        }
    }

    public sealed class LegendItem : INotifyPropertyChanged
    {
        private bool _isVisible;

        public LegendItem(string title, Brush stroke, IReadOnlyList<PieSeries> series, IReadOnlyList<PieChart> charts, bool isVisible)
        {
            Title = title;
            Stroke = stroke;
            Series = series;
            Charts = charts;
            _isVisible = isVisible;
        }

        public string Title { get; }

        public Brush Stroke { get; }

        public IReadOnlyList<PieSeries> Series { get; }

        public IReadOnlyList<PieChart> Charts { get; }

        public Thickness ItemMargin => new(0);

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value)
                    return;

                _isVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
