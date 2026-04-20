using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Workspace.Diagnostics;

internal sealed class WorkspaceSessionMilestoneRecorder
{
    private readonly MainWindowViewModel _viewModel;

    public WorkspaceSessionMilestoneRecorder(MainWindowViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    public void RecordSessionMilestone(string kind, string outcome, string? note = null)
    {
        _viewModel.ChartState.RecordSessionMilestone(new SessionMilestoneSnapshot
        {
            TimestampUtc = DateTime.UtcNow,
            Kind = kind,
            Outcome = outcome,
            MetricType = _viewModel.MetricState.SelectedMetricType,
            SelectedSeriesCount = _viewModel.MetricState.SelectedSeries.Count,
            SelectedDisplayKeys = _viewModel.MetricState.SelectedSeries.Select(series => series.DisplayKey).ToList(),
            RuntimePath = _viewModel.ChartState.LastLoadRuntime?.RuntimePath,
            LoadedSeriesCount = _viewModel.ChartState.LastContext?.ActualSeriesCount ?? 0,
            ContextSignature = EvidenceDiagnosticsBuilder.BuildContextSignature(_viewModel.ChartState.LastContext),
            Note = note
        });
    }
}
