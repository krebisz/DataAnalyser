using DataVisualiser.Core.Data;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Admin;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.UI.Admin;

public sealed class AdminSessionMilestoneRecorderTests
{
    [Fact]
    public void RecordReloadCompleted_ShouldWriteAdminMilestoneToSharedChartState()
    {
        var context = CreateContext();
        context.MetricState.SelectedMetricType = "Weight";
        context.MetricState.SetSeriesSelections([new MetricSeriesSelection("Weight", "body_fat_mass", "Weight", "Fat")]);
        var recorder = new AdminSessionMilestoneRecorder(() => context);

        recorder.RecordReloadCompleted("Weight", 12);

        var milestone = Assert.Single(context.ChartState.SessionMilestones);
        Assert.Equal("AdminReloadCompleted", milestone.Kind);
        Assert.Equal("Success", milestone.Outcome);
        Assert.Equal("Weight", milestone.MetricType);
        Assert.Equal(1, milestone.SelectedSeriesCount);
        Assert.Contains("12 admin row(s)", milestone.Note, StringComparison.Ordinal);
        Assert.Contains("Weight", milestone.Note, StringComparison.Ordinal);
    }

    [Fact]
    public void RecordAdminActions_ShouldUseStableMilestoneKindsForSmokeExports()
    {
        var context = CreateContext();
        var recorder = new AdminSessionMilestoneRecorder(() => context);

        recorder.RecordMetricTypeChanged("BodyComposition");
        recorder.RecordHideDisabledToggled(true);
        recorder.RecordReloadRequested(null);
        recorder.RecordGridEdited("Weight", "body_fat_mass", "MetricSubtypeName");
        recorder.RecordSaveRequested(1);
        recorder.RecordSaveCompleted(1, 1);

        Assert.Equal(
        [
            "AdminMetricTypeChanged",
            "AdminHideDisabledToggled",
            "AdminReloadRequested",
            "AdminGridEdited",
            "AdminSaveRequested",
            "AdminSaveCompleted"
        ], context.ChartState.SessionMilestones.Select(milestone => milestone.Kind));
    }

    [Fact]
    public void RecordFailureAndSkippedSave_ShouldWriteOutcomeAndReason()
    {
        var context = CreateContext();
        var recorder = new AdminSessionMilestoneRecorder(() => context);

        recorder.RecordSaveSkipped();
        recorder.RecordReloadFailed("Weight", "boom");
        recorder.RecordSaveFailed(2, "blocked");

        Assert.Collection(
            context.ChartState.SessionMilestones,
            skipped =>
            {
                Assert.Equal("AdminSaveSkipped", skipped.Kind);
                Assert.Equal("Info", skipped.Outcome);
            },
            reloadFailed =>
            {
                Assert.Equal("AdminReloadFailed", reloadFailed.Kind);
                Assert.Equal("Error", reloadFailed.Outcome);
                Assert.Contains("boom", reloadFailed.Note, StringComparison.Ordinal);
            },
            saveFailed =>
            {
                Assert.Equal("AdminSaveFailed", saveFailed.Kind);
                Assert.Equal("Error", saveFailed.Outcome);
                Assert.Contains("blocked", saveFailed.Note, StringComparison.Ordinal);
            });
    }

    [Fact]
    public void Record_ShouldNoOpWhenSharedContextIsUnavailable()
    {
        var recorder = new AdminSessionMilestoneRecorder(() => null);

        recorder.RecordReloadRequested("Weight");
    }

    private static SharedMainWindowViewModelContext CreateContext()
    {
        var chartState = new ChartState();
        var metricState = new MetricState();
        var uiState = new UiState();
        var metricService = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);

        return new SharedMainWindowViewModelContext(viewModel, chartState, metricState, uiState, metricService);
    }

    private sealed class StubMetricSelectionDataQueries : IMetricSelectionDataQueries
    {
        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null) => Task.FromResult(0L);
        public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype, DateTime? from, DateTime? to, string tableName, int? maxRecords = null, SamplingMode samplingMode = SamplingMode.None, int? targetSamples = null) => Task.FromResult<IEnumerable<MetricData>>([]);
        public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>([]);
        public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>([]);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
    }
}
