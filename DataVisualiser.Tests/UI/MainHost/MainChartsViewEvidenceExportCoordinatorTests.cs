using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.MainHost.Export;
using DataVisualiser.UI.State;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewEvidenceExportCoordinatorTests
{
    [Fact]
    public void ClearEvidence_ShouldInvokeClearAction()
    {
        var cleared = false;
        var coordinator = new MainChartsViewEvidenceExportCoordinator();

        coordinator.ClearEvidence(CreateActions(clearEvidence: () => cleared = true));

        Assert.True(cleared);
    }

    [Fact]
    public async Task ExportAsync_ShouldShowInformationalAndWarningMessagesFromResult()
    {
        var infos = new List<(string Title, string Message)>();
        var warnings = new List<(string Title, string Message)>();
        var coordinator = new MainChartsViewEvidenceExportCoordinator();

        await coordinator.ExportAsync(
            new ChartState(),
            new MetricState(),
            new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc),
            CreateActions(
                exportAsync: (_, _, _) => Task.FromResult(
                    new ReachabilityEvidenceExportResult(
                        @"C:\temp\reachability.json",
                        false,
                        ["warning-1", "warning-2"],
                        [])),
                showInfo: (title, message) => infos.Add((title, message)),
                showWarning: (title, message) => warnings.Add((title, message))));

        Assert.Equal(2, infos.Count);
        Assert.Contains(infos, entry => entry.Title == "Reachability Export" && entry.Message.Contains("No reachability records captured yet.", StringComparison.Ordinal));
        Assert.Contains(infos, entry => entry.Title == "Reachability Export" && entry.Message.Contains(@"C:\temp\reachability.json", StringComparison.Ordinal));
        Assert.Single(warnings);
        Assert.Contains("warning-1", warnings[0].Message, StringComparison.Ordinal);
        Assert.Contains("warning-2", warnings[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExportAsync_ShouldOnlyShowSuccessMessageWhenRecordsExistAndNoWarnings()
    {
        var infos = new List<(string Title, string Message)>();
        var warnings = new List<(string Title, string Message)>();
        var coordinator = new MainChartsViewEvidenceExportCoordinator();

        await coordinator.ExportAsync(
            new ChartState(),
            new MetricState(),
            DateTime.UtcNow,
            CreateActions(
                exportAsync: (_, _, _) => Task.FromResult(
                    new ReachabilityEvidenceExportResult(
                        @"C:\temp\reachability.json",
                        true,
                        [],
                        [])),
                showInfo: (title, message) => infos.Add((title, message)),
                showWarning: (title, message) => warnings.Add((title, message))));

        Assert.Single(infos);
        Assert.Empty(warnings);
    }

    [Fact]
    public async Task ExportAsync_ShouldShowErrorMessageWhenExportFails()
    {
        var errors = new List<(string Title, string Message)>();
        var coordinator = new MainChartsViewEvidenceExportCoordinator();

        await coordinator.ExportAsync(
            new ChartState(),
            new MetricState(),
            DateTime.UtcNow,
            CreateActions(
                exportAsync: (_, _, _) => throw new InvalidOperationException("boom"),
                showError: (title, message) => errors.Add((title, message))));

        Assert.Single(errors);
        Assert.Equal("Reachability Export", errors[0].Title);
        Assert.Contains("boom", errors[0].Message, StringComparison.Ordinal);
    }

    private static MainChartsViewEvidenceExportCoordinator.Actions CreateActions(
        Func<ChartState, MetricState, DateTime, Task<ReachabilityEvidenceExportResult>>? exportAsync = null,
        Action? clearEvidence = null,
        Action<string, string>? showInfo = null,
        Action<string, string>? showWarning = null,
        Action<string, string>? showError = null)
    {
        return new MainChartsViewEvidenceExportCoordinator.Actions(
            exportAsync ?? ((_, _, _) => Task.FromResult(new ReachabilityEvidenceExportResult("path", true, [], []))),
            clearEvidence ?? (() => { }),
            showInfo ?? ((_, _) => { }),
            showWarning ?? ((_, _) => { }),
            showError ?? ((_, _) => { }));
    }
}
