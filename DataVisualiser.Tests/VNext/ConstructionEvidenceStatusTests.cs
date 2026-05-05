using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class ConstructionEvidenceStatusTests
{
    [Fact]
    public void OperationChainEvidence_ShouldDefaultToRetainedObservationalStatus()
    {
        var evidence = new OperationChainEvidence(
            "source",
            "plan",
            "trace",
            "contract",
            ["Weight:morning"],
            ["sum"],
            new Dictionary<string, string>());

        Assert.Equal(ConstructionEvidenceStatus.Retained, evidence.Status);
    }

    [Theory]
    [InlineData(ConstructionEvidenceStatus.PromotionCandidate)]
    [InlineData(ConstructionEvidenceStatus.Quarantined)]
    [InlineData(ConstructionEvidenceStatus.Rejected)]
    public void OperationChainEvidence_ShouldCarryExplicitReviewStatusWithoutRuntimePolicy(ConstructionEvidenceStatus status)
    {
        var evidence = new OperationChainEvidence(
            "source",
            "plan",
            "trace",
            "contract",
            ["Weight:morning"],
            ["sum"],
            new Dictionary<string, string>(),
            status);

        Assert.Equal(status, evidence.Status);
        Assert.Equal("source", evidence.SourceSignature);
        Assert.Equal("contract", evidence.ContractSignature);
    }
}
