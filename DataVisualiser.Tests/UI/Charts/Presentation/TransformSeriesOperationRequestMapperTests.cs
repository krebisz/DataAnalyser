using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class TransformSeriesOperationRequestMapperTests
{
    [Fact]
    public void TryCreate_ShouldMapEmptyOperationToIdentity()
    {
        var mapped = TransformSeriesOperationRequestMapper.TryCreate(null, "Fat", null, out var request);

        Assert.True(mapped);
        Assert.NotNull(request);
        Assert.Equal(SeriesOperationKind.Identity, request!.Kind);
        Assert.Equal([0], request.InputIndexes);
        Assert.Equal("Fat", request.Label);
    }

    [Theory]
    [InlineData("Log", SeriesOperationKind.Logarithm, "Log(Fat)")]
    [InlineData("Sqrt", SeriesOperationKind.SquareRoot, "Sqrt(Fat)")]
    public void TryCreate_ShouldMapSupportedUnaryOperations(string operationTag, SeriesOperationKind expectedKind, string expectedLabel)
    {
        var mapped = TransformSeriesOperationRequestMapper.TryCreate(operationTag, "Fat", null, out var request);

        Assert.True(mapped);
        Assert.NotNull(request);
        Assert.Equal(expectedKind, request!.Kind);
        Assert.Equal([0], request.InputIndexes);
        Assert.Equal(expectedLabel, request.Label);
    }

    [Theory]
    [InlineData("Add", SeriesOperationKind.Sum, "Fat + Muscle")]
    [InlineData("Subtract", SeriesOperationKind.Difference, "Fat - Muscle")]
    [InlineData("Divide", SeriesOperationKind.Ratio, "Fat / Muscle")]
    public void TryCreate_ShouldMapSupportedBinaryOperations(string operationTag, SeriesOperationKind expectedKind, string expectedLabel)
    {
        var mapped = TransformSeriesOperationRequestMapper.TryCreate(operationTag, "Fat", "Muscle", out var request);

        Assert.True(mapped);
        Assert.NotNull(request);
        Assert.Equal(expectedKind, request!.Kind);
        Assert.Equal([0, 1], request.InputIndexes);
        Assert.Equal(expectedLabel, request.Label);
    }

    [Fact]
    public void TryCreate_ShouldRejectCustomOperationsNotRepresentedByVNextKernel()
    {
        var mapped = TransformSeriesOperationRequestMapper.TryCreate("Custom", "Fat", "Muscle", out var request);

        Assert.False(mapped);
        Assert.Null(request);
    }

    [Theory]
    [InlineData(null, SeriesOperationKind.Identity, SeriesOperationRules.Lossless, true)]
    [InlineData("Log", SeriesOperationKind.Logarithm, SeriesOperationRules.Lossless, false)]
    [InlineData("Add", SeriesOperationKind.Sum, SeriesOperationRules.Lossless, false)]
    public void TryCreateOperationChainStep_ShouldExposeTransformOperationAsBoundedOperationChainStep(
        string? operationTag,
        SeriesOperationKind expectedKind,
        string expectedLossiness,
        bool expectedReversible)
    {
        var mapped = TransformSeriesOperationRequestMapper.TryCreateOperationChainStep(operationTag, "Fat", "Muscle", out var step);

        Assert.True(mapped);
        Assert.NotNull(step);
        Assert.Equal(expectedKind, step!.Operation.Kind);
        Assert.Equal(expectedLossiness, step.Lossiness);
        Assert.Equal(expectedReversible, step.Reversible);
        Assert.Equal("TransformTab", step.Metadata["Source"]);
        Assert.Equal(operationTag ?? string.Empty, step.Metadata["OperationTag"]);
    }

    [Fact]
    public void TryCreateOperationChainStep_ShouldRejectUnsupportedOperations()
    {
        var mapped = TransformSeriesOperationRequestMapper.TryCreateOperationChainStep("Custom", "Fat", "Muscle", out var step);

        Assert.False(mapped);
        Assert.Null(step);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("None")]
    public void IsPassThrough_ShouldAcceptEmptyAndNoneOperations(string? operationTag)
    {
        Assert.True(TransformSeriesOperationRequestMapper.IsPassThrough(operationTag));
    }

    [Fact]
    public void TryCreateOperationChainStep_ShouldMapWorkbenchTernarySeriesOperation()
    {
        var mapped = TransformSeriesOperationRequestMapper.TryCreateOperationChainStep(
            "Sum3",
            [
                new MetricSeriesRequest("Weight", "fat", "Weight", "Fat"),
                new MetricSeriesRequest("Weight", "muscle", "Weight", "Muscle"),
                new MetricSeriesRequest("Weight", "bone", "Weight", "Bone")
            ],
            out var step);

        Assert.True(mapped);
        Assert.NotNull(step);
        Assert.Equal(SeriesOperationKind.Sum, step!.Operation.Kind);
        Assert.Equal([0, 1, 2], step.Operation.InputIndexes);
        Assert.Equal("Fat + Muscle + Bone", step.Operation.Label);
        Assert.Equal("OperationChainWorkbench", step.Metadata["Source"]);
        Assert.Equal("Sum3", step.Metadata["OperationTag"]);
    }

    [Fact]
    public void TryCreateOperationChainStep_ShouldRejectWorkbenchBinaryOperationWithoutSecondSeries()
    {
        var mapped = TransformSeriesOperationRequestMapper.TryCreateOperationChainStep(
            "Add",
            [new MetricSeriesRequest("Weight", "fat", "Weight", "Fat")],
            out var step);

        Assert.False(mapped);
        Assert.Null(step);
    }
}
