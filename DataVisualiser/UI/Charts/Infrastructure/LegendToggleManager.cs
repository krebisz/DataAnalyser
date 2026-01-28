using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Adapters;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Infrastructure;

public sealed class LegendToggleManager
{
    private readonly CartesianChart _chart;
    private readonly IDictionary<string, bool>? _visibilityStore;

    public LegendToggleManager(CartesianChart chart, IDictionary<string, bool>? visibilityStore = null)
    {
        _chart = chart ?? throw new ArgumentNullException(nameof(chart));
        _visibilityStore = visibilityStore;
        _chart.Series ??= new SeriesCollection();
        _chart.Series.CollectionChanged += OnSeriesCollectionChanged;
        RebuildItems();
    }

    public ObservableCollection<LegendItem> Items { get; } = new();

    public void AttachItemsControl(ItemsControl itemsControl)
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
        item.Series.Visibility = item.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        item.StoreVisibility?.Invoke(item.IsVisible);
        item.Chart.Update(true, true);
    }

    public static ItemsControl CreateLegendItemsControl(RoutedEventHandler toggleHandler)
    {
        var itemsControl = new ItemsControl
        {
                HorizontalAlignment = HorizontalAlignment.Left
        };

        itemsControl.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(StackPanel)));

        var toggleFactory = new FrameworkElementFactory(typeof(ToggleButton));
        toggleFactory.SetBinding(ToggleButton.IsCheckedProperty,
                new Binding(nameof(LegendItem.IsVisible))
                {
                        Mode = BindingMode.TwoWay
                });
        toggleFactory.AddHandler(ButtonBase.ClickEvent, toggleHandler);
        toggleFactory.SetValue(Control.BackgroundProperty, Brushes.Transparent);
        toggleFactory.SetValue(Control.BorderThicknessProperty, new Thickness(0));
        toggleFactory.SetValue(Control.PaddingProperty, new Thickness(2));
        toggleFactory.SetValue(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left);

        var stackFactory = new FrameworkElementFactory(typeof(StackPanel));
        stackFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        stackFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);

        var rectFactory = new FrameworkElementFactory(typeof(Rectangle));
        rectFactory.SetValue(FrameworkElement.WidthProperty, 12.0);
        rectFactory.SetValue(FrameworkElement.HeightProperty, 12.0);
        rectFactory.SetBinding(Shape.FillProperty, new Binding(nameof(LegendItem.Stroke)));
        rectFactory.SetValue(Shape.StrokeProperty, Brushes.White);
        rectFactory.SetValue(Shape.StrokeThicknessProperty, 0.5);
        rectFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 6, 0));

        var textFactory = new FrameworkElementFactory(typeof(TextBlock));
        textFactory.SetBinding(TextBlock.TextProperty, new Binding(nameof(LegendItem.Title)));
        textFactory.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        textFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Left);

        stackFactory.AppendChild(rectFactory);
        stackFactory.AppendChild(textFactory);
        toggleFactory.AppendChild(stackFactory);

        itemsControl.ItemTemplate = new DataTemplate
        {
                VisualTree = toggleFactory
        };

        return itemsControl;
    }

    public static Border CreateLegendContainer(ItemsControl itemsControl)
    {
        return new Border
        {
                Background = new SolidColorBrush(Color.FromArgb(0xCC, 0x11, 0x11, 0x11)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6),
                Margin = new Thickness(0, 10, 10, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Child = itemsControl
        };
    }

    private void OnSeriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildItems();
    }

    private void RebuildItems()
    {
        Items.Clear();

        foreach (var series in _chart.Series.OfType<Series>())
        {
            var stroke = series.Stroke ?? Brushes.Gray;
            var title = string.IsNullOrWhiteSpace(series.Title) ? "Series" : series.Title;
            var isVisible = series.Visibility != Visibility.Collapsed;

            if (_visibilityStore != null && _visibilityStore.TryGetValue(title, out var storedVisible))
            {
                isVisible = storedVisible;
                series.Visibility = storedVisible ? Visibility.Visible : Visibility.Collapsed;
            }

            Action<bool>? storeVisibility = null;
            if (_visibilityStore != null)
                storeVisibility = value => _visibilityStore[title] = value;

            Items.Add(new LegendItem(_chart, series, title, stroke, isVisible, storeVisibility));
        }
    }

    public sealed class LegendItem : INotifyPropertyChanged
    {
        private bool _isVisible;

        public LegendItem(CartesianChart chart, Series series, string title, Brush stroke, bool isVisible, Action<bool>? storeVisibility)
        {
            Chart = chart;
            Series = series;
            Title = title;
            Stroke = stroke;
            _isVisible = isVisible;
            StoreVisibility = storeVisibility;
        }

        public CartesianChart Chart { get; }

        public Series Series { get; }

        public string Title { get; }

        public Brush Stroke { get; }

        public Action<bool>? StoreVisibility { get; }

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
