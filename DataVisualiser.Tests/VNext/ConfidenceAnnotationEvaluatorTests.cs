using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class ConfidenceAnnotationEvaluatorTests
{
    [Fact]
    public void Evaluate_ShouldReturnEmptySetForCleanExecutionResult()
    {
        var result = CreateResult(
            new MetricSeriesSnapshot(
                new MetricSeriesRequest("Weight", "morning"),
                [new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 1), Value = 1m }],
                null),
            new ChartSeriesProgram("morning", "Morning", [1d], [1d]));

        var annotations = new ConfidenceAnnotationEvaluator().Evaluate(result);

        Assert.False(annotations.HasAnnotations);
        Assert.Equal(result.Program.SourceSignature, annotations.SourceSignature);
        Assert.Equal($"{result.Program.SourceSignature}::", annotations.Signature);
    }

    [Fact]
    public void Evaluate_ShouldAnnotateMissingSeriesDataWithoutMutatingProgramValues()
    {
        var result = CreateResult(
            new MetricSeriesSnapshot(
                new MetricSeriesRequest("Weight", "morning"),
                [],
                null),
            new ChartSeriesProgram("morning", "Morning", [1d], [1d]));

        var annotations = new ConfidenceAnnotationEvaluator().Evaluate(result);

        var annotation = Assert.Single(annotations.Annotations);
        Assert.Equal(ConfidenceAnnotationKind.MissingSeriesData, annotation.Kind);
        Assert.Equal(ConfidenceSeverity.Warning, annotation.Severity);
        Assert.Equal("morning", annotation.SeriesId);
        Assert.Null(annotation.PointIndex);
        Assert.Equal([1d], result.Program.Series[0].RawValues);
    }

    [Fact]
    public void Evaluate_ShouldAnnotateNonFiniteProgramValues()
    {
        var result = CreateResult(
            new MetricSeriesSnapshot(
                new MetricSeriesRequest("Weight", "morning"),
                [new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 1), Value = 1m }],
                null),
            new ChartSeriesProgram("morning", "Morning", [1d, double.NaN], [double.PositiveInfinity, 2d]));

        var annotations = new ConfidenceAnnotationEvaluator().Evaluate(result);

        Assert.Equal(2, annotations.Annotations.Count);
        Assert.Equal(2, annotations.CriticalCount);
        Assert.Contains(annotations.Annotations, annotation =>
            annotation.Kind == ConfidenceAnnotationKind.NonFiniteRawValue &&
            annotation.SeriesId == "morning" &&
            annotation.PointIndex == 1);
        Assert.Contains(annotations.Annotations, annotation =>
            annotation.Kind == ConfidenceAnnotationKind.NonFiniteSmoothedValue &&
            annotation.SeriesId == "morning" &&
            annotation.PointIndex == 0);
    }

    private static AnalyticalExecutionResult CreateResult(
        MetricSeriesSnapshot seriesSnapshot,
        ChartSeriesProgram seriesProgram)
    {
        var selection = new MetricSelectionRequest(
            "Weight",
            [seriesSnapshot.Request],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");
        var snapshot = new MetricLoadSnapshot(selection, [seriesSnapshot], new DateTime(2026, 1, 3));
        var program = new ChartProgram(
            ChartProgramKind.Main,
            ChartDisplayMode.Regular,
            "Weight",
            selection.From,
            selection.To,
            [selection.From, selection.To],
            [seriesProgram],
            selection.Signature);
        var intent = AnalyticalIntent.FromRequests(selection, ChartProgramRequest.MainProgram());

        return new AnalyticalExecutionResult(intent, snapshot, program);
    }
}
