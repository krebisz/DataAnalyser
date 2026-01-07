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
///     Builds a Monday->Sunday min/max stacked column chart for a single metric.
///     Baseline (transparent) = min per day, range column = (max - min) per day.
/// </summary>
public class WeeklyDistributionService : BaseDistributionService
{
    public WeeklyDistributionService(Dictionary<CartesianChart, List<DateTime>> chartTimestamps, IStrategyCutOverService strategyCutOverService, IIntervalShadingStrategy? shadingStrategy = null) : base(new WeeklyDistributionConfiguration(), chartTimestamps, strategyCutOverService, shadingStrategy)
    {
    }

    protected override BucketDistributionResult? ExtractExtendedResult(object strategy)
    {
        return strategy switch
        {
                WeeklyDistributionStrategy legacy => legacy.ExtendedResult,
                CmsWeeklyDistributionStrategy cms => cms.ExtendedResult,
                _                                 => null
        };
    }

    protected override void SetupTooltip(CartesianChart targetChart, ChartComputationResult result, BucketDistributionResult extendedResult, bool useFrequencyShading, int intervalCount)
    {
        Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> tooltipData;
        if (useFrequencyShading)
            tooltipData = CalculateTooltipData(result, extendedResult, intervalCount);
        else
            tooltipData = CalculateSimpleRangeTooltipData(result, extendedResult);

        if (tooltipData != null && tooltipData.Count > 0)
        {
            var oldTooltip = targetChart.Tag as WeeklyDistributionTooltip;
            oldTooltip?.Dispose();

            var tooltip = new WeeklyDistributionTooltip(targetChart, tooltipData);
            targetChart.Tag = tooltip;
        }
        else
        {
            var oldTooltip = targetChart.Tag as WeeklyDistributionTooltip;
            oldTooltip?.Dispose();
            targetChart.Tag = null;
        }
    }

    protected override IIntervalRenderer CreateIntervalRenderer()
    {
        return new WeeklyIntervalRenderer();
    }
}