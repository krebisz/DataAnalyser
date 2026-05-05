using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class SeriesOperationRulesTests
{
    [Theory]
    [InlineData(SeriesOperationKind.Identity, 1, 1)]
    [InlineData(SeriesOperationKind.Normalize, 1, 1)]
    [InlineData(SeriesOperationKind.Logarithm, 1, 1)]
    [InlineData(SeriesOperationKind.SquareRoot, 1, 1)]
    [InlineData(SeriesOperationKind.MovingAverage, 1, 1)]
    [InlineData(SeriesOperationKind.Difference, 2, 2)]
    [InlineData(SeriesOperationKind.Ratio, 2, 2)]
    public void InputCountRules_ShouldExposeProvenArity(SeriesOperationKind kind, int minimum, int maximum)
    {
        Assert.Equal(minimum, SeriesOperationRules.MinimumInputCount(kind));
        Assert.Equal(maximum, SeriesOperationRules.MaximumInputCount(kind));
    }

    [Fact]
    public void Sum_ShouldAllowVariableArity()
    {
        Assert.Equal(1, SeriesOperationRules.MinimumInputCount(SeriesOperationKind.Sum));
        Assert.Null(SeriesOperationRules.MaximumInputCount(SeriesOperationKind.Sum));

        var operation = SeriesOperationRequest.Sum([0, 1, 2], "Total");

        Assert.Equal([0, 1, 2], operation.InputIndexes);
    }

    [Fact]
    public void SeriesOperationRequest_ShouldRejectInvalidArityAndNegativeIndexes()
    {
        Assert.Throws<ArgumentException>(() => new SeriesOperationRequest(SeriesOperationKind.Difference, [0], "bad", "Bad"));
        Assert.Throws<ArgumentException>(() => new SeriesOperationRequest(SeriesOperationKind.Normalize, [0, 1], "bad", "Bad"));
        Assert.Throws<ArgumentException>(() => new SeriesOperationRequest(SeriesOperationKind.Sum, [-1], "bad", "Bad"));
    }

    [Fact]
    public void LossinessRules_ShouldClassifyMovingAverageOnlyAsLossyForCurrentOperations()
    {
        Assert.True(SeriesOperationRules.IsLossy(SeriesOperationKind.MovingAverage));
        Assert.Equal(SeriesOperationRules.WindowedSmoothing, SeriesOperationRules.DefaultLossiness(SeriesOperationKind.MovingAverage));
        Assert.False(SeriesOperationRules.IsLossy(SeriesOperationKind.Ratio));
        Assert.Equal(SeriesOperationRules.Lossless, SeriesOperationRules.DefaultLossiness(SeriesOperationKind.Ratio));
    }
}
