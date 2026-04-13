using System.Windows;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.MainHost.Evidence;

internal sealed class MainChartsSessionDiagnosticsRecorder
{
    private const int MaxTrackedHostMessages = 20;
    private readonly List<HostMessageDiagnosticsSnapshot> _recentHostMessages = [];
    private readonly MainWindowViewModel _viewModel;

    public MainChartsSessionDiagnosticsRecorder(MainWindowViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    public IReadOnlyList<HostMessageDiagnosticsSnapshot> RecentHostMessages => _recentHostMessages;

    public void TrackHostMessage(string title, string message, MessageBoxImage image)
    {
        var severity = image switch
        {
            MessageBoxImage.Error => "Error",
            MessageBoxImage.Warning => "Warning",
            MessageBoxImage.Information => "Information",
            _ => "None"
        };

        _recentHostMessages.Add(new HostMessageDiagnosticsSnapshot
        {
            TimestampUtc = DateTime.UtcNow,
            Severity = severity,
            Title = title,
            Message = message
        });

        RecordSessionMilestone("HostMessage", severity, $"{title}: {message}");

        if (_recentHostMessages.Count <= MaxTrackedHostMessages)
            return;

        _recentHostMessages.RemoveRange(0, _recentHostMessages.Count - MaxTrackedHostMessages);
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
