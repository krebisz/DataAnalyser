using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class OperationChainPlanningRulesTests
{
    [Fact]
    public void Assess_ShouldProduceDeterministicReplaySignatureAndBoundedCost()
    {
        var selection = CreateSelection();
        var steps = new[]
        {
            OperationChainStep.Lossless(SeriesOperationRequest.Sum([0, 1], "Total")),
            OperationChainStep.Lossless(SeriesOperationRequest.Ratio(2, 1, "Total / Evening"))
        };

        var assessment = OperationChainPlanningRules.Assess(selection, steps);

        Assert.Equal(OperationChainPlanningStatus.WithinBudget, assessment.Status);
        Assert.Equal(2, assessment.StepCount);
        Assert.Equal(4, assessment.WorkingSetSize);
        Assert.Contains(selection.Signature, assessment.ReplaySignature, StringComparison.Ordinal);
        Assert.Contains(steps[0].Signature, assessment.ReplaySignature, StringComparison.Ordinal);
        Assert.Equal("False", assessment.ResolvedMetadata["Authoritative"]);
    }

    [Fact]
    public void Assess_ShouldReportExceededBudgetWithoutRejectingPlanShape()
    {
        var selection = CreateSelection();
        var steps = Enumerable.Range(0, 3)
            .Select(index => OperationChainStep.Lossless(SeriesOperationRequest.Identity(0, $"identity-{index}", $"Identity {index}")))
            .ToArray();

        var assessment = OperationChainPlanningRules.Assess(
            selection,
            steps,
            new OperationChainPlanningBudget(MaxSteps: 2, MaxInputReferences: 10, MaxWorkingSetSize: 10));

        Assert.Equal(OperationChainPlanningStatus.ExceedsBudget, assessment.Status);
        Assert.Equal(3, assessment.StepCount);
    }

    [Fact]
    public void OperationChainRequest_ShouldExposePlanningAssessmentWithoutChangingSignature()
    {
        var request = new OperationChainRequest(
            CreateSelection(),
            [OperationChainStep.Lossless(SeriesOperationRequest.Sum([0, 1], "Total"))],
            title: "Derived Workbench");

        Assert.Equal(OperationChainPlanningStatus.WithinBudget, request.Planning.Status);
        Assert.Contains(request.Selection.Signature, request.Planning.ReplaySignature, StringComparison.Ordinal);
        Assert.Contains(request.Steps[0].Signature, request.Planning.ReplaySignature, StringComparison.Ordinal);
    }

    private static MetricSelectionRequest CreateSelection() =>
        new(
            "Weight",
            [
                new MetricSeriesRequest("Weight", "morning"),
                new MetricSeriesRequest("Weight", "evening")
            ],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 3),
            "HealthMetrics");
}
