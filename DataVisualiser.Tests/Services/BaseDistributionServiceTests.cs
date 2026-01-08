using System.Windows.Media;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Shading;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;
using LiveCharts.Wpf;
using Moq;

namespace DataVisualiser.Tests.Services;

public sealed class BaseDistributionServiceTests
{
    [Fact]
    public void CalculateGlobalMinMax_ShouldHandleNaNAndZeroRanges()
    {
        var service = CreateService();
        var mins = new List<double>
        {
                double.NaN,
                5.0,
                double.NaN,
                double.NaN,
                double.NaN,
                double.NaN,
                double.NaN
        };
        var ranges = new List<double>
        {
                double.NaN,
                0.0,
                double.NaN,
                double.NaN,
                double.NaN,
                double.NaN,
                double.NaN
        };

        var (min, max) = service.PublicCalculateGlobalMinMax(mins, ranges);

        Assert.Equal(5.0, min);
        Assert.Equal(6.0, max);
    }

    [Fact]
    public void CalculateSimpleRangeTooltipData_ShouldSkipEmptyBuckets()
    {
        var service = CreateService();
        var result = new ChartComputationResult
        {
                PrimaryRawValues = new List<double>
                {
                        1.0,
                        double.NaN,
                        double.NaN,
                        double.NaN,
                        double.NaN,
                        double.NaN,
                        double.NaN
                },
                PrimarySmoothed = new List<double>
                {
                        0.5,
                        double.NaN,
                        double.NaN,
                        double.NaN,
                        double.NaN,
                        double.NaN,
                        double.NaN
                }
        };

        var extended = new BucketDistributionResult
        {
                Counts = new List<int>
                {
                        2,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0
                }
        };

        var tooltip = service.PublicCalculateSimpleRangeTooltipData(result, extended);

        Assert.True(tooltip.ContainsKey(0));
        Assert.False(tooltip.ContainsKey(1));
        Assert.Single(tooltip[0]);
    }

    [Fact]
    public void CalculateTooltipData_ShouldReturnPercentagesPerBucket()
    {
        var service = CreateService();
        var result = new ChartComputationResult
        {
                PrimaryRawValues = new List<double>
                {
                        1.0,
                        2.0,
                        double.NaN,
                        double.NaN,
                        double.NaN,
                        double.NaN,
                        double.NaN
                },
                PrimarySmoothed = new List<double>
                {
                        1.0,
                        2.0,
                        double.NaN,
                        double.NaN,
                        double.NaN,
                        double.NaN,
                        double.NaN
                }
        };

        var extended = new BucketDistributionResult
        {
                BucketValues = new Dictionary<int, List<double>>
                {
                        {
                                0, new List<double>
                                {
                                        1.1,
                                        1.9
                                }
                        },
                        {
                                1, new List<double>
                                {
                                        2.1,
                                        3.8
                                }
                        }
                }
        };

        var tooltip = service.PublicCalculateTooltipData(result, extended, 2);

        Assert.Equal(7, tooltip.Count);
        Assert.True(tooltip[0].Sum(t => t.Percentage) > 99.0);
        Assert.True(tooltip[1].Sum(t => t.Percentage) > 99.0);
        for (var i = 2; i < 7; i++)
            Assert.Empty(tooltip[i]);
    }

    private static TestDistributionService CreateService()
    {
        var config = new TestDistributionConfiguration();
        var chartTimestamps = new Dictionary<CartesianChart, List<DateTime>>();
        var cutOverService = new Mock<IStrategyCutOverService>();
        return new TestDistributionService(config, chartTimestamps, cutOverService.Object);
    }

    private sealed class TestDistributionService : BaseDistributionService
    {
        public TestDistributionService(IDistributionConfiguration configuration, Dictionary<CartesianChart, List<DateTime>> chartTimestamps, IStrategyCutOverService strategyCutOverService) : base(configuration, chartTimestamps, strategyCutOverService, new FrequencyBasedShadingStrategy(configuration.BucketCount))
        {
        }

        public(double Min, double Max) PublicCalculateGlobalMinMax(List<double> mins, List<double> ranges)
        {
            return CalculateGlobalMinMax(mins, ranges);
        }

        public Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> PublicCalculateTooltipData(ChartComputationResult result, BucketDistributionResult extendedResult, int intervalCount)
        {
            return CalculateTooltipData(result, extendedResult, intervalCount);
        }

        public Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> PublicCalculateSimpleRangeTooltipData(ChartComputationResult result, BucketDistributionResult extendedResult)
        {
            return CalculateSimpleRangeTooltipData(result, extendedResult);
        }

        protected override BucketDistributionResult? ExtractExtendedResult(object strategy)
        {
            return null;
        }

        protected override void SetupTooltip(CartesianChart targetChart, ChartComputationResult result, BucketDistributionResult extendedResult, bool useFrequencyShading, int intervalCount)
        {
        }

        protected override IIntervalRenderer CreateIntervalRenderer()
        {
            return new NoOpIntervalRenderer();
        }
    }

    private sealed class NoOpIntervalRenderer : IIntervalRenderer
    {
        public int RenderIntervals(CartesianChart chart, List<double> mins, List<double> ranges, List<(double Min, double Max)> intervals, Dictionary<int, Dictionary<int, int>> frequenciesPerBucket, Dictionary<int, Dictionary<int, Color>> colorMap, double uniformIntervalHeight, double[] cumulativeStackHeight, int globalMaxFreq)
        {
            return 0;
        }
    }

    private sealed class TestDistributionConfiguration : IDistributionConfiguration
    {
        public int BucketCount => 7;

        public string[] BucketLabels => new[]
        {
                "A",
                "B",
                "C",
                "D",
                "E",
                "F",
                "G"
        };

        public string XAxisTitle => "Bucket";
        public StrategyType StrategyType => StrategyType.WeeklyDistribution;
        public string LogPrefix => "TestDistribution";
        public string BucketName => "Bucket";
        public string BucketVariableName => "bucket";
    }
}