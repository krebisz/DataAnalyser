using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Pipeline;

/// <summary>
///     Thin entry points for the interactive presentation pipeline documented in
///     <c>documents/DATAVISUALISER_PIPELINE_SPINE.md</c>. All behavior stays in
///     <see cref="MainWindowViewModel" /> and coordinators; this type only names the spine.
/// </summary>
public static class ChartPresentationSpine
{
    /// <summary>Stage: load metrics/CMS, build <c>ChartState.LastContext</c>.</summary>
    public static Task<bool> LoadMetricDataIntoLastContextAsync(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        return viewModel.LoadMetricDataAsync();
    }

    /// <summary>Stage: raise <see cref="MainWindowViewModel.DataLoaded" /> and <see cref="MainWindowViewModel.RequestChartUpdate" /> via <see cref="MainWindowViewModel.LoadDataCommand" />.</summary>
    public static void PublishLastContextAndRequestChartUpdate(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        viewModel.LoadDataCommand.Execute(null);
    }
}
