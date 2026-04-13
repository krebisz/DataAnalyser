using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TransformSessionMilestoneRecorder
{
    private readonly MainWindowViewModel _viewModel;

    public TransformSessionMilestoneRecorder(MainWindowViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    public void RecordExecution(TransformExecutionResult execution, TransformResolutionResult resolution)
    {
        var selectedSeries = _viewModel.MetricState.SelectedSeries.ToList();
        var context = resolution.Context;
        var primarySelection = resolution.Selection.PrimarySelection;
        var secondarySelection = resolution.Selection.SecondarySelection;

        _viewModel.ChartState.RecordSessionMilestone(new SessionMilestoneSnapshot
        {
            TimestampUtc = DateTime.UtcNow,
            Kind = string.IsNullOrWhiteSpace(execution.OperationTag) ? "TransformPrimaryProjectionRendered" : "TransformOperationRendered",
            Outcome = "Success",
            MetricType = _viewModel.MetricState.SelectedMetricType,
            SelectedSeriesCount = selectedSeries.Count,
            SelectedDisplayKeys = selectedSeries.Select(series => series.DisplayKey).ToList(),
            RuntimePath = _viewModel.ChartState.LastLoadRuntime?.RuntimePath,
            LoadedSeriesCount = _viewModel.ChartState.LastContext?.ActualSeriesCount ?? 0,
            ContextSignature = EvidenceDiagnosticsBuilder.BuildContextSignature(_viewModel.ChartState.LastContext),
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
        var selectedSeries = _viewModel.MetricState.SelectedSeries.ToList();
        var context = _viewModel.ChartState.LastContext;
        var isVisible = _viewModel.ChartState.IsTransformPanelVisible;

        _viewModel.ChartState.RecordSessionMilestone(new SessionMilestoneSnapshot
        {
            TimestampUtc = DateTime.UtcNow,
            Kind = "TransformToggleRequested",
            Outcome = "Info",
            MetricType = _viewModel.MetricState.SelectedMetricType,
            SelectedSeriesCount = selectedSeries.Count,
            SelectedDisplayKeys = selectedSeries.Select(series => series.DisplayKey).ToList(),
            RuntimePath = _viewModel.ChartState.LastLoadRuntime?.RuntimePath,
            LoadedSeriesCount = context?.ActualSeriesCount ?? 0,
            ContextSignature = EvidenceDiagnosticsBuilder.BuildContextSignature(context),
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
