using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed partial class TransformSessionMilestoneRecorder
{
    private readonly MainWindowViewModel? _viewModel;
    private readonly Func<SharedMainWindowViewModelContext?>? _sharedContextProvider;

    public TransformSessionMilestoneRecorder(MainWindowViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    public TransformSessionMilestoneRecorder(Func<SharedMainWindowViewModelContext?> sharedContextProvider)
    {
        _sharedContextProvider = sharedContextProvider ?? throw new ArgumentNullException(nameof(sharedContextProvider));
    }

    public TransformSessionMilestoneRecorder()
        : this(() => SharedMainWindowViewModelProvider.Current)
    {
    }

    public void RecordExecution(TransformExecutionResult execution, TransformResolutionResult resolution)
    {
        var viewModel = _viewModel ?? throw new InvalidOperationException("Transform chart milestone recording requires a view model.");
        var selectedSeries = viewModel.MetricState.SelectedSeries.ToList();
        var context = resolution.Context;
        var primarySelection = resolution.Selection.PrimarySelection;
        var secondarySelection = resolution.Selection.SecondarySelection;

        viewModel.ChartState.RecordSessionMilestone(new SessionMilestoneSnapshot
        {
            TimestampUtc = DateTime.UtcNow,
            Kind = string.IsNullOrWhiteSpace(execution.OperationTag) ? "TransformPrimaryProjectionRendered" : "TransformOperationRendered",
            Outcome = "Success",
            MetricType = viewModel.MetricState.SelectedMetricType,
            SelectedSeriesCount = selectedSeries.Count,
            SelectedDisplayKeys = selectedSeries.Select(series => series.DisplayKey).ToList(),
            RuntimePath = viewModel.ChartState.LastLoadRuntime?.RuntimePath,
            LoadedSeriesCount = viewModel.ChartState.LastLoadedData.ActualSeriesCount,
            ContextSignature = viewModel.ChartState.LastLoadedData.ContextSignature,
            Operation = string.IsNullOrWhiteSpace(execution.OperationTag) ? null : execution.OperationTag,
            OperationArity = execution.OperationArity,
            PrimarySeriesDisplayKey = primarySelection?.DisplayKey ?? BuildSeriesDisplayKey(context.PrimaryMetricType ?? context.MetricType, context.PrimarySubtype),
            SecondarySeriesDisplayKey = secondarySelection?.DisplayKey ?? BuildSeriesDisplayKey(context.SecondaryMetricType, context.SecondarySubtype),
            ResultPointCount = execution.Results.Count,
            Note = string.IsNullOrWhiteSpace(execution.OperationTag) ? "Primary data projected without an explicit transform operation." : null
        });
    }

    public void RecordToggle()
    {
        var viewModel = _viewModel ?? throw new InvalidOperationException("Transform chart milestone recording requires a view model.");
        var selectedSeries = viewModel.MetricState.SelectedSeries.ToList();
        var loadedData = viewModel.ChartState.LastLoadedData;
        var isVisible = viewModel.ChartState.IsTransformPanelVisible;

        viewModel.ChartState.RecordSessionMilestone(new SessionMilestoneSnapshot
        {
            TimestampUtc = DateTime.UtcNow,
            Kind = "TransformToggleRequested",
            Outcome = "Info",
            MetricType = viewModel.MetricState.SelectedMetricType,
            SelectedSeriesCount = selectedSeries.Count,
            SelectedDisplayKeys = selectedSeries.Select(series => series.DisplayKey).ToList(),
            RuntimePath = viewModel.ChartState.LastLoadRuntime?.RuntimePath,
            LoadedSeriesCount = loadedData.ActualSeriesCount,
            ContextSignature = loadedData.ContextSignature,
            Note = isVisible ? "Transform panel visible." : "Transform panel hidden."
        });
    }

    private static string? BuildSeriesDisplayKey(string? metricType, string? subtype)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return $"{metricType}:{subtype ?? "<none>"}";
    }
}
