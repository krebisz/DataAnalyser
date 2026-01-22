using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Shading;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Services;

/// <summary>
///     Builds a (0 - n) min/max stacked column chart for a single metric.
///     Baseline (transparent) = min per bucket, range column = (max - min) per bucket.
/// </summary>
public class HourlyDistributionService : BaseDistributionService
{
    public HourlyDistributionService(Dictionary<CartesianChart, List<DateTime>> chartTimestamps, IStrategyCutOverService strategyCutOverService, IIntervalShadingStrategy? shadingStrategy = null) : base(new HourlyDistributionConfiguration(), chartTimestamps, strategyCutOverService, shadingStrategy)
    {
    }

    protected override BucketDistributionResult? ExtractExtendedResult(object strategy)
    {
        return strategy switch
        {
                HourlyDistributionStrategy legacy => legacy.ExtendedResult,
                CmsHourlyDistributionStrategy cms => cms.ExtendedResult,
                _ => null
        };
    }

    protected override void SetupTooltip(CartesianChart targetChart, ChartComputationResult result, BucketDistributionResult extendedResult, bool useFrequencyShading, int intervalCount)
    {
        Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> tooltipData;
        Dictionary<int, double>? averages = null;
        if (useFrequencyShading)
        {
            tooltipData = CalculateTooltipData(result, extendedResult, intervalCount);
        }
        else
        {
            tooltipData = CalculateSimpleRangeTooltipData(result, extendedResult);
            averages = CalculateBucketAverages(extendedResult);
        }

        if (tooltipData != null && tooltipData.Count > 0)
        {
            var oldTooltip = targetChart.Tag as HourlyDistributionTooltip;
            oldTooltip?.Dispose();

            var tooltip = new HourlyDistributionTooltip(targetChart, tooltipData, averages);
            targetChart.Tag = tooltip;
        }
        else
        {
            var oldTooltip = targetChart.Tag as HourlyDistributionTooltip;
            oldTooltip?.Dispose();
            targetChart.Tag = null;
        }
    }

    protected override IIntervalRenderer CreateIntervalRenderer()
    {
        return new HourlyIntervalRenderer();
    }
}