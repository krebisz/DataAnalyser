using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Syncfusion;
using Syncfusion.UI.Xaml.SunburstChart;

namespace DataVisualiser.UI.Charts.Controllers;

public partial class SyncfusionSunburstChartController : UserControl, IChartPanelScaffold, ISyncfusionSunburstChartController
{
    private readonly object _paletteKey = new();
    private readonly Dictionary<string, Brush> _submetricBrushes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> _submetricVisibility = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<SunburstItem> _rawItems = Array.Empty<SunburstItem>();
    private readonly List<SfSunburstChart> _ringCharts = new();
    private readonly Dictionary<SfSunburstChart, (double Inner, double Outer)> _ringRanges = new();
    private readonly Dictionary<SfSunburstChart, BucketTooltipContext> _tooltipContextByChart = new();
    private readonly Dictionary<SfSunburstChart, SunburstTooltipModel> _tooltipModelByChart = new();
    private readonly List<string> _currentBucketLabels = new();
    private int _bucketRingCount = 1;
    private bool _isApplyingFilter;
    private bool _isSettingBucketCount;
    private SfSunburstChart? _lastTooltipChart;

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(object),
        typeof(SyncfusionSunburstChartController),
        new PropertyMetadata(null, OnItemsSourceChanged));

    public SyncfusionSunburstChartController()
    {
        InitializeComponent();

        PanelController.Title = "Syncfusion Sunburst";
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);

        if (ChartPresenter.Content is UIElement chartContent)
        {
            ChartPresenter.Content = null;
            PanelController.SetChartContent(chartContent);
        }

        RootGrid.Children.Remove(BehavioralControlsPanel);
        PanelController.SetBehavioralControls(BehavioralControlsPanel);

        PanelController.IsChartVisible = true;

        InitializeBucketCountItems();
        SunburstHost.SizeChanged += (_, _) =>
        {
            UpdateRingLabels();
        };
        SunburstHost.PreviewMouseMove += OnSunburstHostPreviewMouseMove;
        SunburstHost.MouseMove += OnSunburstHostMouseMove;
        SunburstHost.MouseLeave += (_, _) =>
        {
            HideHoverPopup();
        };
        RenderRingCharts(Array.Empty<SunburstItem>());
    }

    public object? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public ChartPanelController Panel => PanelController;

    public Button ToggleButton => PanelController.ToggleButtonControl;

    public string Title
    {
        get => PanelController.Title;
        set => PanelController.Title = value;
    }

    public bool IsChartVisible
    {
        get => PanelController.IsChartVisible;
        set => PanelController.IsChartVisible = value;
    }

    public IChartRenderingContext? RenderingContext
    {
        get => PanelController.RenderingContext;
        set => PanelController.RenderingContext = value;
    }

    public event EventHandler? ToggleRequested;

    public void SetHeaderControls(UIElement? controls)
    {
        PanelController.SetHeaderControls(controls);
    }

    public void SetBehavioralControls(UIElement? controls)
    {
        PanelController.SetBehavioralControls(controls);
    }

    public void SetChartContent(UIElement? chart)
    {
        PanelController.SetChartContent(chart);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SyncfusionSunburstChartController controller)
            return;

        if (controller._isApplyingFilter)
            return;

        var items = (e.NewValue as IEnumerable<SunburstItem>)?.ToList() ?? new List<SunburstItem>();
        controller._rawItems = items;
        controller.UpdateSubmetricBrushes(items);
        controller.UpdateSubmetricLegend(items);
        controller.ApplySubmetricFilter();
    }

    private void UpdateLegendVisibility()
    {
        SubmetricLegendPanel.Visibility = ShowLegendCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateSubmetricBrushes(IEnumerable<SunburstItem>? items)
    {
        _submetricBrushes.Clear();
        if (items == null)
            return;

        ColourPalette.Reset(_paletteKey);
        foreach (var subtype in items
                     .Select(item => item.Submetric)
                     .Where(value => !string.IsNullOrWhiteSpace(value))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(value => value, StringComparer.OrdinalIgnoreCase))
        {
            var color = ColourPalette.Next(_paletteKey);
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            _submetricBrushes[subtype] = brush;
        }
    }

    private void OnSegmentCreated(object? sender, SunburstSegmentCreatedEventArgs e)
    {
        var segment = e.Segment;
        if (segment == null)
            return;

        var key = segment.Category?.ToString();
        if (!string.IsNullOrWhiteSpace(key) && _submetricBrushes.TryGetValue(key, out var resolved))
        {
            segment.Interior = resolved;
        }
    }

    public event EventHandler<int>? BucketCountChanged;

    public void SetBucketCount(int bucketCount)
    {
        _isSettingBucketCount = true;
        BucketCountCombo.SelectedItem = bucketCount;
        _isSettingBucketCount = false;
        _bucketRingCount = Math.Max(1, bucketCount);
        ApplySubmetricFilter();
    }

    private void InitializeBucketCountItems()
    {
        BucketCountCombo.Items.Clear();
        for (var i = 1; i <= 20; i++)
            BucketCountCombo.Items.Add(i);

        BucketCountCombo.SelectedItem = _bucketRingCount;
        ShowLegendCheckBox.Checked += (_, _) => UpdateLegendVisibility();
        ShowLegendCheckBox.Unchecked += (_, _) => UpdateLegendVisibility();
        UpdateLegendVisibility();
    }

    private void OnBucketCountSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isSettingBucketCount)
            return;

        if (BucketCountCombo.SelectedItem is not int bucketCount)
            return;

        _bucketRingCount = Math.Max(1, bucketCount);
        ApplySubmetricFilter();
        BucketCountChanged?.Invoke(this, bucketCount);
    }

    private void UpdateSubmetricLegend(IReadOnlyList<SunburstItem> items)
    {
        var submetrics = items
            .Select(item => item.Submetric)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var key in _submetricVisibility.Keys.ToList())
            if (!submetrics.Contains(key, StringComparer.OrdinalIgnoreCase))
                _submetricVisibility.Remove(key);

        foreach (var subtype in submetrics)
            if (!_submetricVisibility.ContainsKey(subtype))
                _submetricVisibility[subtype] = true;

        SubmetricLegendPanel.Children.Clear();
        foreach (var subtype in submetrics)
        {
            var isChecked = _submetricVisibility.TryGetValue(subtype, out var visible) && visible;
            var brush = _submetricBrushes.TryGetValue(subtype, out var fill)
                ? fill
                : new SolidColorBrush(Colors.Gray);

            var swatch = new Border
            {
                    Width = 12,
                    Height = 12,
                    Background = brush,
                    Margin = new Thickness(0, 0, 4, 0),
                    CornerRadius = new CornerRadius(2)
            };

            var label = new TextBlock
            {
                    Text = subtype,
                    VerticalAlignment = VerticalAlignment.Center
            };

            var content = new StackPanel
            {
                    Orientation = Orientation.Horizontal,
                    Children = { swatch, label }
            };

            var checkbox = new CheckBox
            {
                    IsChecked = isChecked,
                    Content = content,
                    Margin = new Thickness(0, 0, 12, 0),
                    Tag = subtype
            };

            checkbox.Checked += OnLegendToggleChanged;
            checkbox.Unchecked += OnLegendToggleChanged;

            SubmetricLegendPanel.Children.Add(checkbox);
        }
    }

    private void OnLegendToggleChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkbox)
            return;

        var key = checkbox.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(key))
            return;

        _submetricVisibility[key] = checkbox.IsChecked == true;
        ApplySubmetricFilter();
    }

    private void ApplySubmetricFilter()
    {
        if (_rawItems.Count == 0)
        {
            RenderRingCharts(Array.Empty<SunburstItem>());
            return;
        }

        var visibleItems = _rawItems
            .Where(item => item.Submetric != null && _submetricVisibility.TryGetValue(item.Submetric, out var visible) && visible)
            .ToList();

        RenderRingCharts(visibleItems);
    }

    private void RenderRingCharts(IReadOnlyList<SunburstItem> items)
    {
        _isApplyingFilter = true;
        try
        {
            SunburstHost.Children.Clear();
            _ringCharts.Clear();
            _ringRanges.Clear();
            _tooltipContextByChart.Clear();
            _tooltipModelByChart.Clear();
            _currentBucketLabels.Clear();

            if (_bucketRingCount <= 0)
                return;

            var bucketLabels = items
                .Select(item => item.Bucket)
                .Where(label => !string.IsNullOrWhiteSpace(label))
                .Distinct()
                .ToList();
            _currentBucketLabels.AddRange(bucketLabels);

            for (var i = 0; i < _bucketRingCount; i++)
            {
                var bucketLabel = i < bucketLabels.Count ? bucketLabels[i] : $"Bucket {i + 1}";
                var ringItems = items.Where(item => string.Equals(item.Bucket, bucketLabel, StringComparison.OrdinalIgnoreCase)).ToList();
                var bucketTotal = ringItems.Where(item => double.IsFinite(item.Value)).Sum(item => item.Value);
                foreach (var ringItem in ringItems)
                {
                    ringItem.BucketTotal = bucketTotal;
                    ringItem.PercentText = bucketTotal > 0
                        ? string.Format(CultureInfo.InvariantCulture, "Percent: {0:P1}", ringItem.Value / bucketTotal)
                        : "Percent: n/a";
                }

                var breakdown = BuildBucketBreakdown(ringItems, bucketTotal);
                var tooltipContext = new BucketTooltipContext(bucketLabel, bucketTotal, breakdown);

                var ringChart = CreateRingChart(ringItems, i, _bucketRingCount);
                _tooltipContextByChart[ringChart] = tooltipContext;
                _tooltipModelByChart[ringChart] = BuildRingTooltipModel(tooltipContext);
                _ringCharts.Add(ringChart);
                System.Windows.Controls.Panel.SetZIndex(ringChart, _bucketRingCount - i);
                SunburstHost.Children.Add(ringChart);
            }

            UpdateRingLabels();
        }
        finally
        {
            _isApplyingFilter = false;
        }
    }

    private SfSunburstChart CreateRingChart(IReadOnlyList<SunburstItem> items, int index, int total)
    {
        var chart = new SfSunburstChart
        {
                ValueMemberPath = nameof(SunburstItem.Value),
                Background = Brushes.Transparent
        };

        chart.Levels.Add(new SunburstHierarchicalLevel
        {
                GroupMemberPath = nameof(SunburstItem.Submetric)
        });

        chart.ItemsSource = items;
        chart.SegmentCreated += OnSegmentCreated;

        if (total > 0)
        {
            var inner = (double)index / total;
            var outer = (double)(index + 1) / total;
            chart.InnerRadius = inner;
            chart.Radius = outer;
            _ringRanges[chart] = (inner, outer);
        }

        return chart;
    }

    private IReadOnlyList<BucketBreakdownItem> BuildBucketBreakdown(IReadOnlyList<SunburstItem> ringItems, double bucketTotal)
    {
        if (ringItems.Count == 0)
            return Array.Empty<BucketBreakdownItem>();

        return ringItems
            .GroupBy(item => item.Submetric ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var submetric = string.IsNullOrWhiteSpace(group.Key) ? "(Unknown)" : group.Key;
                var value = group.Where(item => double.IsFinite(item.Value)).Sum(item => item.Value);
                var percent = bucketTotal > 0 ? value / bucketTotal : 0;
                var brush = _submetricBrushes.TryGetValue(submetric, out var fill) ? fill : Brushes.Gray;
                return new BucketBreakdownItem(submetric, value, percent, brush);
            })
            .OrderByDescending(item => item.Value)
            .ToList();
    }

    private SunburstTooltipModel BuildRingTooltipModel(BucketTooltipContext context)
    {
        var periodLabel = $"Period: {context.Label}";
        var bucketTotal = context.Total;
        var breakdown = context.Breakdown;
        var totalText = double.IsFinite(bucketTotal)
            ? string.Format(CultureInfo.CurrentCulture, "Total: {0:N2}", bucketTotal)
            : "Total: n/a";

        var breakdownLines = breakdown.Select(item =>
        {
            var text = string.Format(
                CultureInfo.CurrentCulture,
                "{0}: {1:P1}",
                item.Submetric,
                item.Percent);

            return new BucketBreakdownLine(item.Submetric, text, item.Brush);
        }).ToList();

        return new SunburstTooltipModel(
            periodLabel,
            totalText,
            breakdownLines);
    }

    private void OnSunburstHostPreviewMouseMove(object? sender, MouseEventArgs e)
    {
        // Historically we toggled IsHitTestVisible per-ring on every MouseMove to route events to the correct ring.
        // That is expensive and can also provoke unstable behavior inside 3rd party controls.
        //
        // We now determine the ring purely from mouse radius (see GetRingChartAtMouse) and avoid mutating the controls
        // on every move.
    }

    private void OnSunburstHostMouseMove(object? sender, MouseEventArgs e)
    {
        try
        {
            UpdateHoverPopup(e);
        }
        catch
        {
            // Never let hover/tooltip logic crash the app.
            HideHoverPopup();
        }
    }

    private void UpdateHoverPopup(MouseEventArgs e)
    {
        if (_isApplyingFilter)
        {
            HideHoverPopup();
            return;
        }

        // Always determine the ring by mouse radius rather than relying on Syncfusion's internal hit testing.
        // (Overlaid rings + 3rd party hit testing was unstable and could crash the app.)
        var selected = GetRingChartAtMouse(e);
        if (selected == null)
        {
            HideHoverPopup();
            return;
        }

        if (!_tooltipModelByChart.TryGetValue(selected, out var model))
        {
            HideHoverPopup();
            return;
        }

        var position = e.GetPosition(SunburstHost);

        // Avoid excessive object churn/re-template on high-frequency mouse move events.
        // If the user is still on the same ring, only move the popup.
        if (!ReferenceEquals(_lastTooltipChart, selected) || !HoverPopup.IsOpen)
        {
            HoverPopupContent.Content = model;
            _lastTooltipChart = selected;
        }

        HoverPopup.HorizontalOffset = position.X + 16;
        HoverPopup.VerticalOffset = position.Y + 16;
        HoverPopup.IsOpen = true;
    }

    private SfSunburstChart? GetRingChartAtMouse(MouseEventArgs e)
    {
        if (_ringCharts.Count == 0)
            return null;

        var width = SunburstHost.ActualWidth;
        var height = SunburstHost.ActualHeight;
        if (width <= 0 || height <= 0)
            return null;

        var position = e.GetPosition(SunburstHost);
        var center = new Point(width * 0.5, height * 0.5);
        var dx = position.X - center.X;
        var dy = position.Y - center.Y;
        var distance = Math.Sqrt((dx * dx) + (dy * dy));
        var maxRadius = Math.Min(width, height) * 0.5;
        if (maxRadius <= 0)
            return null;

        var ratio = distance / maxRadius;
        return _ringCharts.FirstOrDefault(chart =>
        {
            if (!_ringRanges.TryGetValue(chart, out var range))
                return false;

            return ratio >= range.Inner && ratio <= range.Outer;
        });
    }


    private void HideHoverPopup()
    {
        HoverPopup.IsOpen = false;
        HoverPopupContent.Content = null;
        _lastTooltipChart = null;
    }

    private void UpdateRingLabels()
    {
        RingLabelCanvas.Children.Clear();

        var width = SunburstHost.ActualWidth;
        var height = SunburstHost.ActualHeight;
        if (width <= 0 || height <= 0 || _bucketRingCount <= 0)
            return;

        var radiusBase = Math.Min(width, height) * 0.5;
        var center = new Point(width * 0.5, height * 0.5);

        for (var i = 0; i < _bucketRingCount; i++)
        {
            var label = i < _currentBucketLabels.Count ? _currentBucketLabels[i] : $"Bucket {i + 1}";
            var inner = (double)i / _bucketRingCount;
            var outer = (double)(i + 1) / _bucketRingCount;
            var mid = (inner + outer) * 0.5 * radiusBase;

            var text = new TextBlock
            {
                    Text = label,
                    Foreground = Brushes.Black,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold
            };

            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var size = text.DesiredSize;

            Canvas.SetLeft(text, center.X + mid + 6);
            Canvas.SetTop(text, center.Y - (size.Height * 0.5));
            RingLabelCanvas.Children.Add(text);
        }
    }

    private sealed record BucketTooltipContext(string Label, double Total, IReadOnlyList<BucketBreakdownItem> Breakdown);

    private sealed record BucketBreakdownItem(string Submetric, double Value, double Percent, Brush Brush);

    private sealed record BucketBreakdownLine(string Submetric, string Text, Brush Brush);

    private sealed class SunburstTooltipModel
    {
        public SunburstTooltipModel(
            string periodText,
            string totalText,
            IReadOnlyList<BucketBreakdownLine> bucketBreakdown)
        {
            PeriodText = periodText;
            TotalText = totalText;
            BucketBreakdown = bucketBreakdown;
        }

        public string PeriodText { get; }
        public string TotalText { get; }
        public IReadOnlyList<BucketBreakdownLine> BucketBreakdown { get; }
    }
}
