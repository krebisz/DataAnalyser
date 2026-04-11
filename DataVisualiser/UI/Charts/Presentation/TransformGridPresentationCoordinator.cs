using System.Windows;
using System.Windows.Controls;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Interfaces;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class TransformGridPresentationCoordinator
{
    public static void PopulateInputGrids(
        ITransformDataPanelController controller,
        ChartDataContext context,
        bool hasAvailableSecondaryInput,
        Action<bool> setBinaryTransformOperationsEnabled,
        bool resetResults = true)
    {
        PopulateGrid(controller.TransformGrid1, controller.TransformGrid1Title, context.Data1, context.DisplayName1 ?? "Primary Data", true);

        var hasSecondary = HasSecondaryData(context) && !string.IsNullOrEmpty(context.SecondarySubtype) && context.Data2 != null;
        if (hasSecondary)
        {
            controller.TransformGrid2Panel.Visibility = Visibility.Visible;
            PopulateGrid(controller.TransformGrid2, controller.TransformGrid2Title, context.Data2, context.DisplayName2 ?? "Secondary Data", false);
        }
        else
        {
            controller.TransformGrid2Panel.Visibility = Visibility.Collapsed;
            controller.TransformGrid2.ItemsSource = null;
        }

        setBinaryTransformOperationsEnabled(hasAvailableSecondaryInput);

        if (resetResults)
            ResetResultState(controller);
    }

    public static void SetBinaryTransformOperationsEnabled(ITransformDataPanelController controller, bool enabled)
    {
        var binaryItems = controller.TransformOperationCombo.Items.Cast<ComboBoxItem>().Where(i =>
        {
            var tag = i.Tag?.ToString();
            return tag == "Add" || tag == "Subtract" || tag == "Divide";
        });

        foreach (var item in binaryItems)
            item.IsEnabled = enabled;
    }

    public static void PopulateResultGrid(ITransformDataPanelController controller, List<object> resultData)
    {
        controller.TransformGrid3.ItemsSource = resultData;
        if (controller.TransformGrid3.Columns.Count < 2)
            return;

        if (controller is DataVisualiser.UI.Charts.Controllers.TransformDataPanelControllerV2)
        {
            controller.TransformGrid3.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);
            controller.TransformGrid3.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);
            return;
        }

        controller.TransformGrid3.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
        controller.TransformGrid3.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
    }

    public static void ShowResultPanels(ITransformDataPanelController controller)
    {
        controller.TransformGrid3Panel.Visibility = Visibility.Visible;
        controller.TransformChartContentPanel.Visibility = Visibility.Visible;
    }

    public static void ResetResultState(ITransformDataPanelController controller)
    {
        controller.TransformGrid3Panel.Visibility = Visibility.Collapsed;
        controller.TransformChartContentPanel.Visibility = Visibility.Collapsed;
        controller.TransformGrid3.ItemsSource = null;
        controller.TransformComputeButton.IsEnabled = false;
    }

    public static void ClearAllGrids(ITransformDataPanelController controller)
    {
        controller.TransformGrid1.ItemsSource = null;
        controller.TransformGrid2.ItemsSource = null;
        controller.TransformGrid3.ItemsSource = null;
        controller.TransformGrid2Panel.Visibility = Visibility.Collapsed;
        controller.TransformGrid3Panel.Visibility = Visibility.Collapsed;
        controller.TransformChartContentPanel.Visibility = Visibility.Collapsed;
        controller.TransformComputeButton.IsEnabled = false;
    }

    public static bool ShouldRenderCharts(ChartDataContext? context)
    {
        return context != null && context.Data1 != null && context.Data1.Any();
    }

    public static bool HasSecondaryData(ChartDataContext context)
    {
        return context.Data2 != null && context.Data2.Any();
    }

    private static void PopulateGrid(DataGrid grid, TextBlock title, IEnumerable<MetricData>? data, string titleText, bool alwaysVisible)
    {
        if (data == null && !alwaysVisible)
            return;

        var rows = data?.Where(d => d.Value.HasValue)
            .OrderBy(d => d.NormalizedTimestamp)
            .Select(d => new
            {
                Timestamp = d.NormalizedTimestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                Value = d.Value!.Value.ToString("F4")
            })
            .ToList();

        grid.ItemsSource = rows;
        title.Text = titleText;

        if (grid.Columns.Count < 2)
            return;

        grid.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
        grid.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
    }
}
