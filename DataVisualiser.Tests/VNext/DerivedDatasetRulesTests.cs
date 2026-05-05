using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class DerivedDatasetRulesTests
{
    [Fact]
    public void DerivedDataset_ShouldPreserveConsumerNeutralIdentityProvenanceAndTraceShape()
    {
        var dataset = new DerivedDataset(
            "sum",
            "Total",
            [new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)],
            [3d, 6d],
            [3d, 6d],
            ["Weight:morning", "Weight:evening"],
            "Sum:sum:0,1:",
            new Dictionary<string, string> { [ConstructionMetadataKeys.StepIndex] = "0" });

        Assert.Equal("sum", dataset.Id);
        Assert.Equal("Total", dataset.Label);
        Assert.Equal(2, dataset.Timeline.Count);
        Assert.Equal(2, dataset.RawValues.Count);
        Assert.Equal(2, dataset.SmoothedValues.Count);
        Assert.Equal(["Weight:morning", "Weight:evening"], dataset.SourceSeriesSignatures);
        Assert.Equal("Sum:sum:0,1:", dataset.OperationSignature);
        Assert.Equal("0", dataset.Metadata[ConstructionMetadataKeys.StepIndex]);
        Assert.Contains(dataset.Relations, relation =>
            relation.Kind == ConstructionRelationKind.SourceSeriesDerivation &&
            relation.SourceId == "Weight:morning" &&
            relation.TargetId == dataset.Id);
        Assert.Contains(dataset.Relations, relation =>
            relation.Kind == ConstructionRelationKind.OperationDerivation &&
            relation.SourceId == dataset.OperationSignature &&
            relation.TargetId == dataset.Id);
        Assert.False(dataset.Confidence.HasAnnotations);
        Assert.Equal(dataset.OperationSignature, dataset.Confidence.SourceSignature);
        Assert.Equal(AnalyticalFitnessStatus.Useful, dataset.Fitness.Status);
        Assert.Equal(DerivedDatasetFitnessRules.FiniteCoverageScenario, dataset.Fitness.Scenario);
    }

    [Fact]
    public void DerivedDataset_ShouldRejectCardinalityDrift()
    {
        Assert.Throws<ArgumentException>(() => new DerivedDataset(
            "sum",
            "Total",
            [new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)],
            [3d],
            [3d, 6d],
            ["Weight:morning"],
            "Sum:sum:0:"));

        Assert.Throws<ArgumentException>(() => new DerivedDataset(
            "sum",
            "Total",
            [new DateTime(2026, 1, 1)],
            [3d],
            [3d, 6d],
            ["Weight:morning"],
            "Sum:sum:0:"));
    }

    [Fact]
    public void DerivedDataset_ShouldRequireSourceProvenanceAndOperationTrace()
    {
        Assert.Throws<ArgumentException>(() => new DerivedDataset(
            "sum",
            "Total",
            [new DateTime(2026, 1, 1)],
            [3d],
            [3d],
            [],
            "Sum:sum:0:"));

        Assert.Throws<ArgumentException>(() => new DerivedDataset(
            "sum",
            "Total",
            [new DateTime(2026, 1, 1)],
            [3d],
            [3d],
            ["Weight:morning"],
            ""));
    }

    [Fact]
    public void DerivedDataset_ShouldAnnotateNonFiniteDerivedValuesWithoutMutatingTruth()
    {
        var dataset = new DerivedDataset(
            "ratio",
            "Morning / Evening",
            [new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)],
            [double.PositiveInfinity, 2d],
            [1d, double.NaN],
            ["Weight:morning", "Weight:evening"],
            "Ratio:ratio:0,1:");

        Assert.Equal([double.PositiveInfinity, 2d], dataset.RawValues);
        Assert.Equal([1d, double.NaN], dataset.SmoothedValues);
        Assert.True(dataset.Confidence.HasAnnotations);
        Assert.Equal(2, dataset.Confidence.CriticalCount);
        Assert.Equal(dataset.OperationSignature, dataset.Confidence.SourceSignature);
        Assert.Contains(dataset.Confidence.Annotations, annotation =>
            annotation.Kind == ConfidenceAnnotationKind.NonFiniteRawValue &&
            annotation.SeriesId == dataset.Id &&
            annotation.PointIndex == 0 &&
            annotation.ResolvedMetadata["Authoritative"] == "False");
        Assert.Contains(dataset.Confidence.Annotations, annotation =>
            annotation.Kind == ConfidenceAnnotationKind.NonFiniteSmoothedValue &&
            annotation.SeriesId == dataset.Id &&
            annotation.PointIndex == 1 &&
            annotation.ResolvedMetadata[ConstructionMetadataKeys.OperationSignature] == dataset.OperationSignature);
        Assert.Equal(AnalyticalFitnessStatus.Caution, dataset.Fitness.Status);
        Assert.Equal("False", dataset.Fitness.ResolvedMetadata["Authoritative"]);
        Assert.Equal(dataset.OperationSignature, dataset.Fitness.ResolvedMetadata[ConstructionMetadataKeys.OperationSignature]);
    }

    [Fact]
    public void DerivedDataset_ShouldSeparateValidConstructionFromDistortionProneFitness()
    {
        var dataset = new DerivedDataset(
            "ratio",
            "Morning / Evening",
            [new DateTime(2026, 1, 1), new DateTime(2026, 1, 2), new DateTime(2026, 1, 3)],
            [double.NaN, double.PositiveInfinity, 2d],
            [double.NaN, double.NaN, 2d],
            ["Weight:morning", "Weight:evening"],
            "Ratio:ratio:0,1:");

        Assert.Equal(3, dataset.RawValues.Count);
        Assert.Equal(3, dataset.SmoothedValues.Count);
        Assert.Equal(AnalyticalFitnessStatus.DistortionProne, dataset.Fitness.Status);
        Assert.Equal("0.333", dataset.Fitness.ResolvedMetadata["SmoothedFiniteCoverage"]);
    }
}
