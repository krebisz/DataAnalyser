using DataFileReader.Canonical;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating MultiMetric strategies.
/// </summary>
public sealed class MultiMetricStrategyFactory : StrategyFactoryBase
{
    public MultiMetricStrategyFactory() : base(CreateCms,
            CreateLegacy)
    {
    }

    private static IChartComputationStrategy CreateCms(DataVisualiser.Core.Orchestration.ChartDataContext ctx, StrategyCreationParameters p)
    {
        var cmsSeries = p.CmsSeries ?? ctx.CmsSeries;
        if (cmsSeries == null || cmsSeries.Count == 0)
            throw new InvalidOperationException("CmsSeries is null for MultiMetric CMS execution.");

        return new MultiMetricStrategy(cmsSeries, p.Labels ?? Array.Empty<string>(), p.From, p.To);
    }

    private static IChartComputationStrategy CreateLegacy(StrategyCreationParameters p)
    {
        return new MultiMetricStrategy(p.LegacySeries ?? Array.Empty<IEnumerable<MetricData>>(), p.Labels ?? Array.Empty<string>(), p.From, p.To, p.Unit);
    }
}
