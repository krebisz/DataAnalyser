using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class ConstructionRelationTests
{
    [Fact]
    public void ConstructionRelation_ShouldKeepDerivationAndProjectionDistinct()
    {
        var derivation = new ConstructionRelation(
            ConstructionRelationKind.SourceSeriesDerivation,
            "Weight:morning",
            "sum");
        var projection = new ConstructionRelation(
            ConstructionRelationKind.Projection,
            "sum",
            "OperationChainWorkbench",
            new Dictionary<string, string> { ["Boundary"] = "Consumer" });

        Assert.Equal(ConstructionRelationKind.SourceSeriesDerivation, derivation.Kind);
        Assert.Equal(ConstructionRelationKind.Projection, projection.Kind);
        Assert.Equal("Consumer", projection.Metadata["Boundary"]);
    }

    [Fact]
    public void ConstructionRelation_ShouldRejectMissingEndpoints()
    {
        Assert.Throws<ArgumentException>(() => new ConstructionRelation(
            ConstructionRelationKind.SourceSeriesDerivation,
            "",
            "sum"));

        Assert.Throws<ArgumentException>(() => new ConstructionRelation(
            ConstructionRelationKind.OperationDerivation,
            "Sum:sum:0,1:",
            ""));
    }
}
