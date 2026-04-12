using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Tests.Services;

public sealed class DistributionRangeResultBuilderTests
{
    [Fact]
    public void Build_ShouldReturnNull_WhenResultShapeIsInvalid()
    {
        var result = new ChartComputationResult
        {
            PrimaryRawValues = [1.0],
            PrimarySmoothed = [0.5, 0.25]
        };

        var built = DistributionRangeResultBuilder.Build(result, null, 2);

        Assert.Null(built);
    }

    [Fact]
    public void Build_ShouldComputeMaximaAndAverages()
    {
        var result = new ChartComputationResult
        {
            PrimaryRawValues = [1.0, 2.0],
            PrimarySmoothed = [0.5, double.NaN],
            Unit = "kg"
        };
        var extended = new BucketDistributionResult
        {
            BucketValues = new Dictionary<int, List<double>>
            {
                [0] = [1.0, 2.0],
                [1] = [double.NaN, 4.0]
            }
        };

        var built = DistributionRangeResultBuilder.Build(result, extended, 2);

        Assert.NotNull(built);
        Assert.Equal([1.0, 2.0], built!.Mins);
        Assert.Equal([1.5, 2.0], built.Maxs);
        Assert.Equal([1.5, 4.0], built.Averages);
        Assert.Equal(1.0, built.GlobalMin);
        Assert.Equal(2.0, built.GlobalMax);
        Assert.Equal("kg", built.Unit);
    }
}
