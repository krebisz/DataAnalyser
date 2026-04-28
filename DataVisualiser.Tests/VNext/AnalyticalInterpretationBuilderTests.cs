using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class AnalyticalInterpretationBuilderTests
{
    [Fact]
    public void Build_ShouldComposeExecutionConfidenceAndRequestedOverlays()
    {
        var execution = CreateExecution(new ChartSeriesProgram("morning", "Morning", [1d, 2d, double.NaN], [1d, 2d, 3d]));

        var interpretation = new AnalyticalInterpretationBuilder().Build(
            execution,
            includeAverageLines: true,
            includeMedianLines: true);

        Assert.Same(execution, interpretation.Execution);
        Assert.True(interpretation.Confidence.HasAnnotations);
        Assert.Equal(2, interpretation.Overlays.Count);
        Assert.Contains(interpretation.Overlays, overlay => overlay.Kind == OverlayKind.AverageLine);
        Assert.Contains(interpretation.Overlays, overlay => overlay.Kind == OverlayKind.MedianLine);
        Assert.Contains(execution.Signature, interpretation.Signature, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_ShouldNotCreateOverlaysUnlessRequested()
    {
        var execution = CreateExecution(new ChartSeriesProgram("morning", "Morning", [1d, 2d], [1d, 2d]));

        var interpretation = new AnalyticalInterpretationBuilder().Build(execution);

        Assert.Empty(interpretation.Overlays);
        Assert.False(interpretation.Confidence.HasAnnotations);
    }

    [Fact]
    public void Build_ShouldOptionallyExcludeCriticalConfidenceSeriesFromOverlays()
    {
        var execution = CreateExecution(
            new ChartSeriesProgram("morning", "Morning", [1d, double.NaN], [1d, 2d]),
            new ChartSeriesProgram("evening", "Evening", [3d, 5d], [3d, 5d]));

        var interpretation = new AnalyticalInterpretationBuilder().Build(
            execution,
            new AnalyticalInterpretationOptions(
                IncludeAverageLines: true,
                ExcludeCriticalConfidenceSeriesFromOverlays: true));

        Assert.True(interpretation.Confidence.HasAnnotations);
        Assert.Same(execution, interpretation.Execution);
        Assert.Same(execution.Program, interpretation.Execution.Program);
        Assert.Equal(2, interpretation.Execution.Program.Series.Count);
        var overlay = Assert.Single(interpretation.Overlays);
        Assert.Equal("evening", overlay.ResolvedParameters["SeriesId"]);
        Assert.Equal(2, execution.Program.Series.Count);
    }

    [Fact]
    public void Build_ShouldKeepConfidenceAsAnnotationWithoutChangingProgramTruth()
    {
        var execution = CreateExecution(
            new ChartSeriesProgram("morning", "Morning", [1d, double.NaN], [1d, 2d]));
        var originalProgram = execution.Program;
        var originalSeries = execution.Program.Series[0];

        var interpretation = new AnalyticalInterpretationBuilder().Build(
            execution,
            includeAverageLines: true,
            includeMedianLines: true);

        Assert.Same(execution, interpretation.Execution);
        Assert.Same(originalProgram, interpretation.Execution.Program);
        Assert.Same(originalSeries, interpretation.Execution.Program.Series[0]);
        Assert.Equal([1d, double.NaN], interpretation.Execution.Program.Series[0].RawValues);
        Assert.True(interpretation.Confidence.HasAnnotations);
        Assert.Equal(execution.Program.SourceSignature, interpretation.Confidence.SourceSignature);
        Assert.All(interpretation.Overlays, overlay =>
        {
            Assert.Equal("False", overlay.ResolvedParameters["Authoritative"]);
            Assert.Equal(execution.Program.SourceSignature, overlay.ResolvedParameters["SourceSignature"]);
        });
    }

    [Fact]
    public void BuildSet_ShouldInterpretEveryExecutionResult()
    {
        var execution = CreateExecution(new ChartSeriesProgram("morning", "Morning", [1d, 2d], [1d, 2d]));
        var executionSet = new AnalyticalResultSet(
            execution.Intent.Selection,
            [execution]);

        var result = new AnalyticalInterpretationBuilder().BuildSet(
            executionSet,
            includeAverageLines: true);

        Assert.Same(executionSet, result.ExecutionSet);
        var interpretation = Assert.Single(result.Interpretations);
        Assert.Same(execution, interpretation.Execution);
        Assert.Single(interpretation.Overlays);
        Assert.Contains(executionSet.Signature, result.Signature, StringComparison.Ordinal);
    }

    private static AnalyticalExecutionResult CreateExecution(params ChartSeriesProgram[] seriesPrograms)
    {
        var selection = new MetricSelectionRequest(
            "Weight",
            seriesPrograms.Select(series => new MetricSeriesRequest("Weight", series.Id)).ToArray(),
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 3),
            "HealthMetrics");
        var snapshot = new MetricLoadSnapshot(
            selection,
            selection.Series
                .Select(series => new MetricSeriesSnapshot(
                    series,
                    [new MetricData { NormalizedTimestamp = selection.From, Value = 1m }],
                    null))
                .ToArray(),
            new DateTime(2026, 1, 4));
        var program = new ChartProgram(
            ChartProgramKind.Main,
            ChartDisplayMode.Regular,
            "Weight",
            selection.From,
            selection.To,
            [selection.From, selection.From.AddDays(1), selection.To],
            seriesPrograms,
            selection.Signature);
        var intent = AnalyticalIntent.FromRequests(selection, ChartProgramRequest.MainProgram());

        return new AnalyticalExecutionResult(intent, snapshot, program);
    }
}
