using System.Windows;
using System.Windows.Controls;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.UI.MainHost.Evidence;

internal sealed class MainChartsUiSurfaceDiagnosticsReader
{
    private readonly SubtypeSelectorManager _selectorManager;
    private readonly MainChartsSessionDiagnosticsRecorder _sessionDiagnosticsRecorder;

    public MainChartsUiSurfaceDiagnosticsReader(
        SubtypeSelectorManager selectorManager,
        MainChartsSessionDiagnosticsRecorder sessionDiagnosticsRecorder)
    {
        _selectorManager = selectorManager ?? throw new ArgumentNullException(nameof(selectorManager));
        _sessionDiagnosticsRecorder = sessionDiagnosticsRecorder ?? throw new ArgumentNullException(nameof(sessionDiagnosticsRecorder));
    }

    public UiSurfaceDiagnosticsSnapshot Capture(
        ComboBox metricTypeCombo,
        DatePicker fromDatePicker,
        DatePicker toDatePicker,
        ITransformDataPanelController transformController)
    {
        ArgumentNullException.ThrowIfNull(metricTypeCombo);
        ArgumentNullException.ThrowIfNull(fromDatePicker);
        ArgumentNullException.ThrowIfNull(toDatePicker);
        ArgumentNullException.ThrowIfNull(transformController);

        var selectedMetricValue = MetricSelectionComboReader.GetSelectedMetricValue(metricTypeCombo);
        var selectedMetricOption = MetricSelectionComboReader.GetSelectedMetricOption(metricTypeCombo);
        var expectedDefaultFromDate = DateTime.UtcNow.Date.AddDays(-30);
        var expectedDefaultToDate = DateTime.UtcNow.Date;
        var orderedSubtypeCombos = _selectorManager.GetActiveCombos()
            .Select((combo, index) => new SubtypeComboDiagnosticsSnapshot
            {
                Index = index,
                BoundMetricType = _selectorManager.GetMetricTypeForCombo(combo)?.Value,
                SelectedValue = GetSelectedComboValue(combo),
                SelectedDisplay = GetSelectedComboDisplay(combo),
                OptionCount = combo.Items.Count,
                OptionValues = ExtractComboOptionValues(combo)
            })
            .ToList();

        var controllerElement = transformController as UIElement;
        var transformSnapshot = new TransformUiDiagnosticsSnapshot
        {
            PanelVisible = controllerElement?.Visibility == Visibility.Visible,
            SecondaryPanelVisible = transformController.TransformSecondarySubtypePanel.Visibility == Visibility.Visible,
            ComputeEnabled = transformController.TransformComputeButton.IsEnabled,
            SelectedOperation = GetSelectedOperationTag(transformController.TransformOperationCombo),
            SelectedPrimarySubtype = GetSelectedComboValue(transformController.TransformPrimarySubtypeCombo),
            SelectedSecondarySubtype = GetSelectedComboValue(transformController.TransformSecondarySubtypeCombo),
            PrimaryOptionCount = transformController.TransformPrimarySubtypeCombo.Items.Count,
            SecondaryOptionCount = transformController.TransformSecondarySubtypeCombo.Items.Count
        };

        return new UiSurfaceDiagnosticsSnapshot
        {
            MetricType = new MetricTypeUiDiagnosticsSnapshot
            {
                SelectedValue = selectedMetricValue,
                SelectedDisplay = selectedMetricOption?.Display ?? metricTypeCombo.Text,
                OptionCount = metricTypeCombo.Items.Count
            },
            DateRange = new DateRangeUiDiagnosticsSnapshot
            {
                SelectedFromDate = fromDatePicker.SelectedDate,
                SelectedToDate = toDatePicker.SelectedDate,
                ExpectedDefaultFromDateUtc = expectedDefaultFromDate,
                ExpectedDefaultToDateUtc = expectedDefaultToDate,
                MatchesExpectedDefaultWindow =
                    fromDatePicker.SelectedDate?.Date == expectedDefaultFromDate &&
                    toDatePicker.SelectedDate?.Date == expectedDefaultToDate
            },
            Subtypes = new SubtypeUiDiagnosticsSnapshot
            {
                ActiveComboCount = orderedSubtypeCombos.Count,
                PrimarySelectionMaterialized = _selectorManager.PrimaryCombo.SelectedItem != null,
                AllCombosBoundToSelectedMetricType = orderedSubtypeCombos.All(combo =>
                    string.Equals(combo.BoundMetricType, selectedMetricValue, StringComparison.OrdinalIgnoreCase)),
                OrderedCombos = orderedSubtypeCombos
            },
            Transform = transformSnapshot,
            RecentMessages = _sessionDiagnosticsRecorder.RecentHostMessages.ToList()
        };
    }

    internal static string? GetSelectedComboValue(ComboBox combo)
    {
        return (combo.SelectedItem as MetricNameOption)?.Value ??
               combo.SelectedValue?.ToString() ??
               combo.SelectedItem?.ToString();
    }

    internal static string? GetSelectedComboDisplay(ComboBox combo)
    {
        return (combo.SelectedItem as MetricNameOption)?.Display ??
               combo.SelectedItem?.ToString();
    }

    internal static IReadOnlyList<string> ExtractComboOptionValues(ComboBox combo)
    {
        return combo.Items
            .OfType<MetricNameOption>()
            .Select(option => option.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();
    }

    internal static string? GetSelectedOperationTag(ComboBox combo)
    {
        return combo.SelectedItem is ComboBoxItem item ? item.Tag?.ToString() : null;
    }
}
