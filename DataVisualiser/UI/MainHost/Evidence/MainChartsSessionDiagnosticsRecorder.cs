using System.Windows;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.UI.Workspace.Diagnostics;

namespace DataVisualiser.UI.MainHost.Evidence;

internal sealed class MainChartsSessionDiagnosticsRecorder
{
    private const int MaxTrackedHostMessages = 20;
    private readonly List<HostMessageDiagnosticsSnapshot> _recentHostMessages = [];
    private readonly WorkspaceSessionMilestoneRecorder _milestoneRecorder;

    public MainChartsSessionDiagnosticsRecorder(MainWindowViewModel viewModel)
    {
        _milestoneRecorder = new WorkspaceSessionMilestoneRecorder(viewModel);
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
        _milestoneRecorder.RecordSessionMilestone(kind, outcome, note);
    }
}
