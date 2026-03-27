using DataVisualiser.Shared.Helpers;

namespace DataVisualiser.Tests.Helpers;

public sealed class FrequencyBinningHelperTests
{
    [Fact]
    public void CalculateBinSize_ShouldReturnFallback_WhenRangeIsInvalid()
    {
        var result = FrequencyBinningHelper.CalculateBinSize(5.0, 5.0);

        Assert.Equal(1.0, result);
    }

    [Fact]
    public void CreateBins_ShouldIncludeLastBoundaryBin()
    {
        var bins = FrequencyBinningHelper.CreateBins(0.0, 2.0, 1.0);

        Assert.Equal(2, bins.Count);
        Assert.Equal((1.0, 2.0), bins[^1]);
    }

    [Fact]
    public void BinValuesAndCountFrequencies_ShouldSkipInvalidValues_AndIncludeMaxValueInLastBin()
    {
        var bins = FrequencyBinningHelper.CreateBins(0.0, 2.0, 1.0);
        var values = new List<double>
        {
                0.2,
                0.8,
                1.2,
                2.0,
                double.NaN,
                double.PositiveInfinity
        };

        var frequencies = FrequencyBinningHelper.BinValuesAndCountFrequencies(values, bins);

        Assert.Equal(2, frequencies[0]);
        Assert.Equal(2, frequencies[1]);
    }

    [Fact]
    public void NormalizeFrequencies_ShouldReturnZeros_WhenGlobalMaximumIsZero()
    {
        var frequencies = new Dictionary<int, Dictionary<int, int>>
        {
                [0] = new Dictionary<int, int>
                {
                        [0] = 0,
                        [1] = 0
                }
        };

        var normalized = FrequencyBinningHelper.NormalizeFrequencies(frequencies);

        Assert.Equal(0.0, normalized[0][0]);
        Assert.Equal(0.0, normalized[0][1]);
    }
}
