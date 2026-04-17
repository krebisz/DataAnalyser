using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Core.Transforms;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Interfaces;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class TransformChartPresentationCoordinator
{
    public static async Task RenderResultsAsync(
        ITransformDataPanelController controller,
        ITransformRenderingContract renderingContract,
        TransformChartRenderHost renderHost,
        List<MetricData> dataList,
        List<double> results,
        string operation,
        List<IReadOnlyList<MetricData>> metrics,
        ChartDataContext transformContext,
        string? overrideLabel = null)
    {
        if (dataList.Count == 0 || results.Count == 0)
            return;

        await PrepareChartLayoutAsync(controller);
        await RenderTransformChartAsync(controller, renderingContract, renderHost, dataList, results, operation, metrics, transformContext, overrideLabel);
        await FinalizeChartRenderingAsync(controller);
    }

    private static async Task PrepareChartLayoutAsync(ITransformDataPanelController controller)
    {
        controller.TransformChartContentPanel.UpdateLayout();
        await controller.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
        await CalculateAndSetTransformChartWidthAsync(controller);
        Debug.WriteLine($"[TransformChart] Before render - ActualWidth={controller.ChartTransformResult.ActualWidth}, ActualHeight={controller.ChartTransformResult.ActualHeight}, IsVisible={controller.ChartTransformResult.IsVisible}, PanelVisible={controller.TransformChartContentPanel.Visibility}");
    }

    private static async Task FinalizeChartRenderingAsync(ITransformDataPanelController controller)
    {
        controller.ChartTransformResult.Update(true, true);
        controller.TransformChartContentPanel.UpdateLayout();
        await controller.Dispatcher.InvokeAsync(() =>
        {
            controller.ChartTransformResult.InvalidateVisual();
            controller.ChartTransformResult.Update(true, true);
        }, DispatcherPriority.Render);
        Debug.WriteLine($"[TransformChart] After render - ActualWidth={controller.ChartTransformResult.ActualWidth}, ActualHeight={controller.ChartTransformResult.ActualHeight}, SeriesCount={controller.ChartTransformResult.Series?.Count ?? 0}");
    }

    private static async Task CalculateAndSetTransformChartWidthAsync(ITransformDataPanelController controller)
    {
        await controller.Dispatcher.InvokeAsync(() =>
        {
            if (controller.TransformChartContainer == null)
                return;

            if (controller is ITransformLayoutCapabilities { UsesAutomaticChartWidth: true })
            {
                controller.TransformChartContainer.ClearValue(FrameworkElement.WidthProperty);
                return;
            }

            var parentStackPanel = controller.TransformChartContainer.Parent as FrameworkElement;
            if (parentStackPanel?.Parent is not FrameworkElement parentContainer)
                return;

            var usedWidth = CalculateUsedWidthForTransformGrids(controller) + 40;
            var availableWidth = parentContainer.ActualWidth > 0 ? parentContainer.ActualWidth : 1800;
            var chartWidth = Math.Max(400, availableWidth - usedWidth - 40);
            controller.TransformChartContainer.Width = chartWidth;

            Debug.WriteLine($"[TransformChart] Calculated width - parentWidth={parentContainer.ActualWidth}, usedWidth={usedWidth}, chartWidth={chartWidth}");
        }, DispatcherPriority.Render);
    }

    private static double CalculateUsedWidthForTransformGrids(ITransformDataPanelController controller)
    {
        double usedWidth = 0;

        var grid1StackPanel = controller.TransformGrid1.Parent as FrameworkElement;
        usedWidth += grid1StackPanel?.ActualWidth > 0 ? grid1StackPanel.ActualWidth : 250;

        if (controller.TransformGrid2Panel.IsVisible)
            usedWidth += controller.TransformGrid2Panel.ActualWidth > 0 ? controller.TransformGrid2Panel.ActualWidth : 250;

        if (controller.TransformGrid3Panel.IsVisible)
            usedWidth += controller.TransformGrid3Panel.ActualWidth > 0 ? controller.TransformGrid3Panel.ActualWidth : 250;

        return usedWidth;
    }

    private static async Task RenderTransformChartAsync(
        ITransformDataPanelController controller,
        ITransformRenderingContract renderingContract,
        TransformChartRenderHost renderHost,
        List<MetricData> dataList,
        List<double> results,
        string operation,
        List<IReadOnlyList<MetricData>> metrics,
        ChartDataContext transformContext,
        string? overrideLabel)
    {
        var from = transformContext.From != default ? transformContext.From : dataList.Min(d => d.NormalizedTimestamp);
        var to = transformContext.To != default ? transformContext.To : dataList.Max(d => d.NormalizedTimestamp);
        var label = overrideLabel ?? TransformExpressionEvaluator.GenerateTransformLabel(operation, metrics, transformContext);

        var strategy = new TransformResultStrategy(dataList, results, label, from, to);
        var operationType = operation == "Subtract" ? "-" : operation == "Add" ? "+" : operation == "Divide" ? "/" : null;
        var isOperationChart = operation == "Subtract" || operation == "Add" || operation == "Divide";

        await renderingContract.RenderAsync(
            new TransformChartRenderRequest(
                TransformRenderingRoute.ResultCartesian,
                transformContext,
                strategy,
                label,
                operationType,
                isOperationChart),
            renderHost);
    }
}
