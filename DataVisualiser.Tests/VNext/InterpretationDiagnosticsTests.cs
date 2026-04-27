using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class InterpretationDiagnosticsTests
{
    [Fact]
    public void RecordInterpretationDiagnostics_StoresContractSummary()
    {
        var state = new ChartState();
        var interpretation = CreateInterpretation(
            ChartProgramKind.Transform,
            [double.NaN, 2d],
            includeAverageLines: true,
            includeMedianLines: true);

        state.RecordInterpretationDiagnostics(interpretation);

        var snapshot = Assert.Single(state.InterpretationDiagnostics);
        Assert.Equal("Transform", snapshot.ProgramKind);
        Assert.Equal(interpretation.Execution.Signature, snapshot.ExecutionSignature);
        Assert.Equal(interpretation.Signature, snapshot.InterpretationSignature);
        Assert.Equal(interpretation.Execution.Program.SourceSignature, snapshot.SourceSignature);
        Assert.Equal(1, snapshot.ConfidenceAnnotationCount);
        Assert.Equal(1, snapshot.CriticalConfidenceAnnotationCount);
        Assert.Equal(0, snapshot.WarningConfidenceAnnotationCount);
        Assert.Equal(2, snapshot.OverlayCount);
        Assert.Equal(["AverageLine", "MedianLine"], snapshot.OverlayKinds);
    }

    [Fact]
    public void ClearInterpretationDiagnostics_RemovesRecordedSummaries()
    {
        var state = new ChartState();
        state.RecordInterpretationDiagnostics(CreateInterpretation(ChartProgramKind.Main, [1d, 2d]));

        state.ClearInterpretationDiagnostics();

        Assert.Empty(state.InterpretationDiagnostics);
    }

    [Fact]
    public void RecordInterpretationDiagnostics_CapsHistory()
    {
        var state = new ChartState();

        for (var i = 0; i < 105; i++)
            state.RecordInterpretationDiagnostics(CreateInterpretation(ChartProgramKind.Main, [1d, i]));

        Assert.Equal(100, state.InterpretationDiagnostics.Count);
    }

    [Fact]
    public void BuildInterpretationDiagnostics_AggregatesRecordedSummaries()
    {
        var state = new ChartState();
        var main = CreateInterpretation(ChartProgramKind.Main, [1d, 2d], includeAverageLines: true);
        var transform = CreateInterpretation(ChartProgramKind.Transform, [double.NaN, 3d], includeMedianLines: true);
        state.RecordInterpretationDiagnostics(main);
        state.RecordInterpretationDiagnostics(transform);

        var diagnostics = EvidenceDiagnosticsBuilder.BuildInterpretationDiagnostics(state);

        Assert.Equal(2, diagnostics.InterpretationCount);
        Assert.Equal(1, diagnostics.TotalConfidenceAnnotations);
        Assert.Equal(1, diagnostics.CriticalConfidenceAnnotations);
        Assert.Equal(0, diagnostics.WarningConfidenceAnnotations);
        Assert.True(diagnostics.HasCriticalConfidence);
        Assert.Equal(2, diagnostics.TotalOverlays);
        Assert.Equal(["Main", "Transform"], diagnostics.ProgramKinds);
        Assert.Equal(["AverageLine", "MedianLine"], diagnostics.OverlayKinds);
        Assert.Equal(transform.Signature, diagnostics.LatestInterpretationSignature);
        Assert.Equal(2, diagnostics.Results.Count);
    }

    private static AnalyticalInterpretationResult CreateInterpretation(
        ChartProgramKind kind,
        IReadOnlyList<double> values,
        bool includeAverageLines = false,
        bool includeMedianLines = false)
    {
        var selection = new MetricSelectionRequest(
            "Weight",
            [new MetricSeriesRequest("Weight", "mass")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");
        var snapshot = new MetricLoadSnapshot(
            selection,
            [
                new MetricSeriesSnapshot(
                    selection.Series[0],
                    [new MetricData { NormalizedTimestamp = selection.From, Value = 1m }],
                    null)
            ],
            new DateTime(2026, 1, 3));
        var program = new ChartProgram(
            kind,
            ChartDisplayMode.Regular,
            "Weight",
            selection.From,
            selection.To,
            [selection.From, selection.To],
            [new ChartSeriesProgram("mass", "Mass", values, [1d, 2d])],
            selection.Signature);
        var intent = AnalyticalIntent.FromRequests(selection, new ChartProgramRequest(kind, ChartDisplayMode.Regular));
        var execution = new AnalyticalExecutionResult(intent, snapshot, program);

        return new AnalyticalInterpretationBuilder().Build(
            execution,
            includeAverageLines,
            includeMedianLines);
    }
}
