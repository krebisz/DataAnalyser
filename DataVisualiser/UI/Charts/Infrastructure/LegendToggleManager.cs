using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Adapters;
using System;
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
        RecalculateStackedAxisIfNeeded(item.Chart);
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
        toggleFactory.SetBinding(FrameworkElement.MarginProperty, new Binding(nameof(LegendItem.ItemMargin)));
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

        var stackingState = _chart.Tag as ChartStackingTooltipState;

        var orderedSeries = _chart.Series
            .OfType<Series>()
            .OrderBy(series =>
            {
                var title = series.Title ?? string.Empty;
                if (title.EndsWith(" (smooth)", StringComparison.OrdinalIgnoreCase))
                    return 0;
                if (title.EndsWith(" (raw)", StringComparison.OrdinalIgnoreCase))
                    return 1;
                return 2;
            })
            .ThenBy(series => series.Title ?? string.Empty, StringComparer.OrdinalIgnoreCase);

        var hasSmoothed = false;
        var insertedRawSpacer = false;

        foreach (var series in orderedSeries)
        {
            var stroke = series.Stroke ?? Brushes.Gray;
            var title = string.IsNullOrWhiteSpace(series.Title) ? "Series" : series.Title;
            var isVisible = series.Visibility != Visibility.Collapsed;
            var isOverlay = IsOverlaySeries(series, stackingState);

            var itemMargin = new Thickness(0);
            if (title.EndsWith(" (smooth)", StringComparison.OrdinalIgnoreCase))
                hasSmoothed = true;
            else if (!insertedRawSpacer && hasSmoothed && title.EndsWith(" (raw)", StringComparison.OrdinalIgnoreCase))
            {
                itemMargin = new Thickness(0, 6, 0, 0);
                insertedRawSpacer = true;
            }

            if (_visibilityStore != null && !isOverlay && _visibilityStore.TryGetValue(title, out var storedVisible))
            {
                isVisible = storedVisible;
                series.Visibility = storedVisible ? Visibility.Visible : Visibility.Collapsed;
            }

            Action<bool>? storeVisibility = null;
            if (_visibilityStore != null && !isOverlay)
                storeVisibility = value => _visibilityStore[title] = value;

            Items.Add(new LegendItem(_chart, series, title, stroke, isVisible, storeVisibility, itemMargin));
        }
    }

    private static bool IsOverlaySeries(Series series, ChartStackingTooltipState? state)
    {
        if (state?.OverlaySeriesNames == null)
            return false;

        var title = series.Title ?? string.Empty;
        var baseName = TrimSeriesSuffix(title);
        if (string.IsNullOrWhiteSpace(baseName))
            return false;

        return state.OverlaySeriesNames.Contains(ChartStackingTooltipState.NormalizeOverlayName(baseName), StringComparer.OrdinalIgnoreCase);
    }

    private static string TrimSeriesSuffix(string title)
    {
        return ChartStackingTooltipState.NormalizeOverlayName(title);
    }

    private static void RecalculateStackedAxisIfNeeded(CartesianChart chart)
    {
        if (chart.Tag is not ChartStackingTooltipState)
            return;

        if (chart.AxisY.Count == 0)
            return;

        var stackingState = chart.Tag as ChartStackingTooltipState;

        var stackedSeries = chart.Series
            .OfType<StackedAreaSeries>()
            .Where(series => series.Visibility == Visibility.Visible)
            .ToList();

        var overlayValues = CollectOverlayValues(chart, stackingState);

        var metrics = new List<MetricData>();

        if (stackedSeries.Count > 0)
        {
            var maxCount = stackedSeries.Max(series => series.Values?.Count ?? 0);
            if (maxCount > 0)
            {
                var totals = new double[maxCount];
                foreach (var series in stackedSeries)
                {
                    if (series.Values == null)
                        continue;

                    var values = series.Values.Cast<double>().ToList();
                    for (var i = 0; i < values.Count; i++)
                    {
                        var value = values[i];
                        if (double.IsNaN(value) || double.IsInfinity(value))
                            continue;

                        totals[i] += value;
                    }
                }

                for (var i = 0; i < totals.Length; i++)
                    metrics.Add(new MetricData
                    {
                            NormalizedTimestamp = DateTime.UtcNow,
                            Value = (decimal)totals[i]
                    });

                var min = totals.Where(total => !double.IsNaN(total) && !double.IsInfinity(total))
                    .DefaultIfEmpty(double.NaN)
                    .Min();

                if (!double.IsNaN(min) && min > 0)
                {
                    metrics.Add(new MetricData
                    {
                            NormalizedTimestamp = DateTime.UtcNow,
                            Value = 0m
                    });
                }
            }
        }

        if (metrics.Count == 0 && overlayValues.Count == 0)
            return;

        var yAxis = chart.AxisY[0];
        ChartHelper.NormalizeYAxis(yAxis, metrics, overlayValues);
        ChartHelper.ApplyTransformChartGradient(chart, yAxis);
    }

    private static List<double> CollectOverlayValues(CartesianChart chart, ChartStackingTooltipState? state)
    {
        var values = new List<double>();
        if (state?.OverlaySeriesNames == null)
            return values;

        foreach (var series in chart.Series.OfType<Series>())
        {
            var title = series.Title ?? string.Empty;
            var baseName = ChartStackingTooltipState.NormalizeOverlayName(title);
            if (!state.OverlaySeriesNames.Contains(baseName, StringComparer.OrdinalIgnoreCase))
                continue;

            if (series.Values == null)
                continue;

            foreach (var value in series.Values.Cast<double>())
                if (!double.IsNaN(value) && !double.IsInfinity(value))
                    values.Add(value);
        }

        return values;
    }

    public sealed class LegendItem : INotifyPropertyChanged
    {
        private bool _isVisible;

        public LegendItem(CartesianChart chart, Series series, string title, Brush stroke, bool isVisible, Action<bool>? storeVisibility, Thickness itemMargin)
        {
            Chart = chart;
            Series = series;
            Title = title;
            Stroke = stroke;
            _isVisible = isVisible;
            StoreVisibility = storeVisibility;
            ItemMargin = itemMargin;
        }

        public CartesianChart Chart { get; }

        public Series Series { get; }

        public string Title { get; }

        public Brush Stroke { get; }

        public Action<bool>? StoreVisibility { get; }

        public Thickness ItemMargin { get; }

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
