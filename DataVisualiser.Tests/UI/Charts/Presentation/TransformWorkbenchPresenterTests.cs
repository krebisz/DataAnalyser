using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class TransformWorkbenchPresenterTests
{
    [Fact]
    public void Build_ShouldProjectResultToDisplayRowsWithoutExecutingOperations()
    {
        var result = CreateResult();

        var presentation = TransformWorkbenchPresenter.Build(result);

        Assert.Equal("1 derived dataset(s) from 2 source series.", presentation.Summary);
        var traceRow = Assert.Single(presentation.TraceRows);
        Assert.Equal("Step 1", traceRow.StepLabel);
        Assert.Equal("Sum -> sum; lossiness: Lossless", traceRow.Detail);
        Assert.Equal(2, presentation.DatasetRows.Count);
        Assert.Equal("Total", presentation.DatasetRows[0].Series);
        Assert.Equal("2026-01-01 00:00:00", presentation.DatasetRows[0].Timestamp);
        Assert.Equal("3.0000", presentation.DatasetRows[0].Raw);
        Assert.Equal("NaN", presentation.DatasetRows[1].Smoothed);
        Assert.Equal("Plan: plan-sig | Trace: trace-sig | Contract: contract-sig", presentation.Evidence);
    }

    private static OperationChainResult CreateResult()
    {
        var selection = new MetricSelectionRequest(
            "Weight",
            [
                new MetricSeriesRequest("Weight", "morning"),
                new MetricSeriesRequest("Weight", "evening")
            ],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");
        var step = OperationChainStep.Lossless(
            SeriesOperationRequest.Sum([0, 1], "Total"),
            reversible: true);
        var request = new OperationChainRequest(selection, [step], title: "Workbench");
        var program = new OperationChainProgram(
            request.Signature,
            request.Title,
            request.Selection,
            request.Steps,
            request.Delivery);
        var plan = new OperationChainExecutionPlan(
            program,
            "source-sig",
            ["Weight:morning", "Weight:evening"],
            [step.Operation]);
        var dataset = new DerivedDataset(
            "sum",
            "Total",
            [new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)],
            [3d, 6d],
            [3d, double.NaN],
            ["Weight:morning", "Weight:evening"],
            "Sum:sum:0,1:",
            new Dictionary<string, string>());
        var trace = new OperationChainTrace(
        [
            new OperationChainTraceEntry(
                0,
                SeriesOperationKind.Sum,
                [0, 1],
                dataset.Id,
                Reversible: true,
                Lossiness: "Lossless",
                Metadata: new Dictionary<string, string>())
        ]);
        var evidence = new OperationChainEvidence(
            "source-sig",
            "plan-sig",
            "trace-sig",
            "contract-sig",
            plan.SourceSeriesSignatures,
            [dataset.Id],
            new Dictionary<string, string>());

        return new OperationChainResult(
            request,
            plan,
            [dataset],
            trace,
            evidence,
            null!);
    }
}
