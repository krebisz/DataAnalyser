using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Tests.VNext;

public sealed class AnalyticalIntentContractsTests
{
    [Fact]
    public void FromRequests_ShouldTranslateExistingSelectionAndProgramRequestsIntoIntent()
    {
        var selection = CreateSelection();
        var programRequest = ChartProgramRequest.MainProgram(ChartDisplayMode.Stacked);
        var overlay = new OverlayPlan(OverlayKind.AverageLine, "Average");
        var interaction = new InteractionRequest(InteractionKind.ResetZoom, "MainChart");

        var intent = AnalyticalIntent.FromRequests(
            selection,
            programRequest,
            overlays: [overlay],
            interactions: [interaction]);

        Assert.Same(selection, intent.Selection);
        Assert.Same(programRequest, intent.ProgramRequest);
        Assert.Equal(ConsumerKind.Chart, intent.Delivery.ConsumerKind);
        Assert.Equal(ChartProgramKind.Main, intent.Delivery.ProgramKind);
        Assert.Equal(selection.Signature, intent.Provenance.SourceSignature);
        Assert.Equal(AnalyticalCapabilityKind.Identity, intent.Capability.CapabilityKind);
        Assert.Equal(CompositionKind.MultiSeries, intent.Capability.CompositionKind);
        Assert.Single(intent.Overlays);
        Assert.Single(intent.Interactions);
        Assert.Contains(selection.Signature, intent.Signature, StringComparison.Ordinal);
        Assert.Contains("Average", intent.Signature, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildProgram_WithAnalyticalIntent_ShouldDelegateToExistingProgramPlanner()
    {
        var planner = new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel());
        var snapshot = CreateSnapshot();
        var intent = AnalyticalIntent.FromRequests(
            snapshot.Request,
            ChartProgramRequest.Difference());

        var program = planner.BuildProgram(snapshot, intent);

        Assert.Equal(ChartProgramKind.Difference, program.Kind);
        Assert.Equal(snapshot.Signature, program.SourceSignature);
        Assert.Single(program.Series);
        Assert.Equal([-1d, -1d], program.Series[0].RawValues);
    }

    [Fact]
    public void BuildProgram_WithAnalyticalIntent_ShouldRejectMismatchedLoadedSnapshot()
    {
        var planner = new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel());
        var snapshot = CreateSnapshot();
        var otherSelection = new MetricSelectionRequest(
            "Sleep",
            [new MetricSeriesRequest("Sleep", "total")],
            snapshot.Request.From,
            snapshot.Request.To,
            snapshot.Request.ResolutionTableName);
        var intent = AnalyticalIntent.FromRequests(otherSelection, ChartProgramRequest.MainProgram());

        var ex = Assert.Throws<InvalidOperationException>(() => planner.BuildProgram(snapshot, intent));

        Assert.Contains("does not match", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ProjectCartesian_WithIntent_ShouldCarryVocabularyMetadataToRenderPlan()
    {
        var snapshot = CreateSnapshot();
        var planner = new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel());
        var intent = AnalyticalIntent.FromRequests(
            snapshot.Request,
            ChartProgramRequest.MainProgram(),
            delivery: ConsumerDeliveryContract.Chart(ChartProgramKind.Main, "MainChart"),
            overlays: [new OverlayPlan(OverlayKind.MedianLine, "Median")],
            interactions: [new InteractionRequest(InteractionKind.ViewportChange, "MainChart")]);
        var program = planner.BuildProgram(snapshot, intent);
        var projector = new ChartRenderPlanProjector();

        var plan = projector.ProjectCartesian(program, intent: intent);

        Assert.Equal(intent.Signature, plan.Metadata[ChartRenderPlanMetadataKeys.IntentSignature]);
        Assert.Equal(intent.Provenance.Signature, plan.Metadata[ChartRenderPlanMetadataKeys.ProvenanceSignature]);
        Assert.Equal(ConsumerKind.Chart.ToString(), plan.Metadata[ChartRenderPlanMetadataKeys.ConsumerKind]);
        Assert.Equal("MainChart", plan.Metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
        Assert.Equal(AnalyticalCapabilityKind.Identity.ToString(), plan.Metadata[ChartRenderPlanMetadataKeys.CapabilityKind]);
        Assert.Equal(CompositionKind.MultiSeries.ToString(), plan.Metadata[ChartRenderPlanMetadataKeys.CompositionKind]);
        Assert.Equal("1", plan.Metadata[ChartRenderPlanMetadataKeys.OverlayCount]);
        Assert.Equal("1", plan.Metadata[ChartRenderPlanMetadataKeys.InteractionCount]);
    }

    [Fact]
    public void ProjectCartesian_WithExecutionResult_ShouldCarryIntentMetadataToRenderPlan()
    {
        var snapshot = CreateSnapshot();
        var planner = new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel());
        var intent = AnalyticalIntent.FromRequests(
            snapshot.Request,
            ChartProgramRequest.Ratio(),
            ConsumerDeliveryContract.Chart(ChartProgramKind.Ratio, "RatioChart"));
        var program = planner.BuildProgram(snapshot, intent);
        var result = new AnalyticalExecutionResult(intent, snapshot, program);
        var projector = new ChartRenderPlanProjector();

        var plan = projector.ProjectCartesian(result);

        Assert.Equal(ChartProgramKind.Ratio, plan.ProgramKind);
        Assert.Equal(intent.Signature, plan.Metadata[ChartRenderPlanMetadataKeys.IntentSignature]);
        Assert.Equal(intent.Provenance.Signature, plan.Metadata[ChartRenderPlanMetadataKeys.ProvenanceSignature]);
        Assert.Equal("RatioChart", plan.Metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
        Assert.Equal(AnalyticalCapabilityKind.Comparison.ToString(), plan.Metadata[ChartRenderPlanMetadataKeys.CapabilityKind]);
        Assert.Equal(CompositionKind.DerivedSeries.ToString(), plan.Metadata[ChartRenderPlanMetadataKeys.CompositionKind]);
    }

    [Theory]
    [InlineData(ChartProgramKind.Main, AnalyticalCapabilityKind.Identity, CompositionKind.MultiSeries)]
    [InlineData(ChartProgramKind.Normalized, AnalyticalCapabilityKind.Normalization, CompositionKind.MultiSeries)]
    [InlineData(ChartProgramKind.Difference, AnalyticalCapabilityKind.Comparison, CompositionKind.DerivedSeries)]
    [InlineData(ChartProgramKind.Ratio, AnalyticalCapabilityKind.Comparison, CompositionKind.DerivedSeries)]
    [InlineData(ChartProgramKind.Distribution, AnalyticalCapabilityKind.Distribution, CompositionKind.SingleSeries)]
    [InlineData(ChartProgramKind.WeekdayTrend, AnalyticalCapabilityKind.TemporalTrend, CompositionKind.SingleSeries)]
    [InlineData(ChartProgramKind.BarPie, AnalyticalCapabilityKind.Identity, CompositionKind.MultiSeries)]
    [InlineData(ChartProgramKind.SyncfusionSunburst, AnalyticalCapabilityKind.Hierarchy, CompositionKind.Hierarchy)]
    public void CapabilityRequest_ShouldInferCapabilityAndCompositionFromProgramKind(
        ChartProgramKind kind,
        AnalyticalCapabilityKind expectedCapability,
        CompositionKind expectedComposition)
    {
        var request = new ChartProgramRequest(kind);

        var capability = CapabilityRequest.FromProgramRequest(request);

        Assert.Equal(expectedCapability, capability.CapabilityKind);
        Assert.Equal(expectedComposition, capability.CompositionKind);
    }

    [Fact]
    public void AnalyticalExecutionResult_ShouldRejectMismatchedProgramSource()
    {
        var snapshot = CreateSnapshot();
        var intent = AnalyticalIntent.FromRequests(snapshot.Request, ChartProgramRequest.MainProgram());
        var program = new ChartProgram(
            ChartProgramKind.Main,
            ChartDisplayMode.Regular,
            "Weight",
            snapshot.Request.From,
            snapshot.Request.To,
            [snapshot.Request.From],
            [new ChartSeriesProgram("x", "X", [1d], [1d])],
            "other-signature");

        var ex = Assert.Throws<ArgumentException>(() => new AnalyticalExecutionResult(intent, snapshot, program));

        Assert.Contains("Program source signature", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ConsumerDeliveryContract_ShouldRejectProgramKindDrift()
    {
        var selection = CreateSelection();
        var delivery = ConsumerDeliveryContract.Chart(ChartProgramKind.Ratio);

        var ex = Assert.Throws<ArgumentException>(() =>
            AnalyticalIntent.FromRequests(selection, ChartProgramRequest.Difference(), delivery));

        Assert.Contains("Delivery program kind", ex.Message, StringComparison.Ordinal);
    }

    private static MetricSelectionRequest CreateSelection()
    {
        return new MetricSelectionRequest(
            "Weight",
            [new MetricSeriesRequest("Weight", "morning"), new MetricSeriesRequest("Weight", "evening")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");
    }

    private static MetricLoadSnapshot CreateSnapshot()
    {
        var request = CreateSelection();

        return new MetricLoadSnapshot(
            request,
            [
                new MetricSeriesSnapshot(
                    request.Series[0],
                    [
                        new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 1), Value = 1m },
                        new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 2), Value = 2m }
                    ],
                    null),
                new MetricSeriesSnapshot(
                    request.Series[1],
                    [
                        new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 1), Value = 2m },
                        new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 2), Value = 3m }
                    ],
                    null)
            ],
            DateTime.UtcNow);
    }
}
