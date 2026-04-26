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
    public void ProvenanceDescriptor_ShouldExposeExplicitAuthorityAndTrustVocabulary()
    {
        var raw = ProvenanceDescriptor.Raw("db:healthmetrics");
        var projected = ProvenanceDescriptor.Projected("program:main");
        var delivered = ProvenanceDescriptor.Delivered("render:main", AnalyticalAuthority.External);

        Assert.Equal("Legacy", raw.Authority);
        Assert.Equal("Raw", raw.TrustClass);
        Assert.Equal("VNext", projected.Authority);
        Assert.Equal("Projected", projected.TrustClass);
        Assert.Equal("External:Delivered:render:main", delivered.Signature);
    }

    [Fact]
    public void ProvenanceDescriptor_FromSelection_ShouldMarkSelectionAsRequestedAuthority()
    {
        var selection = CreateSelection();

        var provenance = ProvenanceDescriptor.FromSelection(selection);

        Assert.Equal(selection.Signature, provenance.SourceSignature);
        Assert.Equal("VNext", provenance.Authority);
        Assert.Equal("Requested", provenance.TrustClass);
        Assert.Equal($"VNext:Requested:{selection.Signature}", provenance.Signature);
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

    [Fact]
    public void ConsumerDeliveryContract_ShouldRepresentExportAndApiConsumersWithoutRenderPlanRequirement()
    {
        var export = ConsumerDeliveryContract.Export(ChartProgramKind.Main);
        var api = ConsumerDeliveryContract.Api(ChartProgramKind.Normalized, "MetricSeriesEndpoint");

        Assert.Equal(ConsumerKind.Export, export.ConsumerKind);
        Assert.Equal("EvidenceExport", export.DeliveryTarget);
        Assert.False(export.RequiresRenderPlan);
        Assert.Equal(ConsumerKind.Api, api.ConsumerKind);
        Assert.Equal("MetricSeriesEndpoint", api.DeliveryTarget);
        Assert.False(api.RequiresRenderPlan);
    }

    [Fact]
    public void AnalyticalResultSet_ShouldGroupMultipleProgramResultsForOneSelection()
    {
        var snapshot = CreateSnapshot();
        var main = CreateExecutionResult(snapshot, ChartProgramRequest.MainProgram());
        var difference = CreateExecutionResult(snapshot, ChartProgramRequest.Difference());

        var resultSet = new AnalyticalResultSet(snapshot.Request, [main, difference]);

        Assert.Equal(snapshot.Request.Signature, resultSet.Selection.Signature);
        Assert.Equal([ChartProgramKind.Main, ChartProgramKind.Difference], resultSet.ProgramKinds);
        Assert.Contains("Main", resultSet.Signature, StringComparison.Ordinal);
        Assert.Contains("Difference", resultSet.Signature, StringComparison.Ordinal);
    }

    [Fact]
    public void AnalyticalIntentSet_ShouldGroupMultipleProgramRequestsForOneSelection()
    {
        var selection = CreateSelection();
        var main = AnalyticalIntent.FromRequests(selection, ChartProgramRequest.MainProgram());
        var normalized = AnalyticalIntent.FromRequests(selection, ChartProgramRequest.Normalized());

        var intentSet = AnalyticalIntentSet.FromIntents([main, normalized]);

        Assert.Equal(selection.Signature, intentSet.Selection.Signature);
        Assert.Equal([ChartProgramKind.Main, ChartProgramKind.Normalized], intentSet.ProgramKinds);
        Assert.Contains("Main", intentSet.Signature, StringComparison.Ordinal);
        Assert.Contains("Normalized", intentSet.Signature, StringComparison.Ordinal);
    }

    [Fact]
    public void AnalyticalIntentSet_ShouldRejectMixedSelections()
    {
        var selection = CreateSelection();
        var otherSelection = new MetricSelectionRequest(
            "Sleep",
            [new MetricSeriesRequest("Sleep", "total")],
            selection.From,
            selection.To,
            selection.ResolutionTableName);
        var main = AnalyticalIntent.FromRequests(selection, ChartProgramRequest.MainProgram());
        var other = AnalyticalIntent.FromRequests(otherSelection, ChartProgramRequest.MainProgram());

        var ex = Assert.Throws<ArgumentException>(() => AnalyticalIntentSet.FromIntents([main, other]));

        Assert.Contains("share the intent-set selection", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnalyticalResultSet_ShouldRejectMixedSelectionResults()
    {
        var snapshot = CreateSnapshot();
        var otherSelection = new MetricSelectionRequest(
            "Sleep",
            [new MetricSeriesRequest("Sleep", "total")],
            snapshot.Request.From,
            snapshot.Request.To,
            snapshot.Request.ResolutionTableName);
        var otherSnapshot = new MetricLoadSnapshot(
            otherSelection,
            [
                new MetricSeriesSnapshot(
                    otherSelection.Series[0],
                    [new MetricData { NormalizedTimestamp = snapshot.Request.From, Value = 8m }],
                    null)
            ],
            DateTime.UtcNow);
        var main = CreateExecutionResult(snapshot, ChartProgramRequest.MainProgram());
        var other = CreateExecutionResult(otherSnapshot, ChartProgramRequest.MainProgram());

        var ex = Assert.Throws<ArgumentException>(() => new AnalyticalResultSet(snapshot.Request, [main, other]));

        Assert.Contains("share the result-set selection", ex.Message, StringComparison.Ordinal);
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

    private static AnalyticalExecutionResult CreateExecutionResult(
        MetricLoadSnapshot snapshot,
        ChartProgramRequest request)
    {
        var planner = new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel());
        var intent = AnalyticalIntent.FromRequests(snapshot.Request, request);
        var program = planner.BuildProgram(snapshot, intent);

        return new AnalyticalExecutionResult(intent, snapshot, program);
    }
}
