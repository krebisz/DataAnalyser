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
}
