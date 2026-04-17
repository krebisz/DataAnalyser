using System.Diagnostics;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Factories;
using DataVisualiser.Core.Strategies.Reachability;
using ParityResult = DataVisualiser.Core.Validation.ParityResult;

namespace DataVisualiser.Core.Strategies;

/// <summary>
///     Implementation of IStrategyCutOverService.
///     Provides unified cut-over mechanism for all strategies with parity validation.
/// </summary>
public sealed class StrategyCutOverService : IStrategyCutOverService
{
    private readonly ICmsRuntimeConfiguration _cmsRuntimeConfiguration;
    private readonly StrategyCmsDecisionEvaluator _cmsDecisionEvaluator;
    private readonly Dictionary<StrategyType, IStrategyFactory> _factories;
    private readonly StrategyParityValidationService _parityValidationService = new();

    private readonly IStrategyReachabilityProbe _reachabilityProbe;

    public StrategyCutOverService(IDataPreparationService dataPreparation, IStrategyReachabilityProbe? reachabilityProbe = null, ICmsRuntimeConfiguration? cmsRuntimeConfiguration = null)
    {
        _ = dataPreparation ?? throw new ArgumentNullException(nameof(dataPreparation));
        _cmsRuntimeConfiguration = cmsRuntimeConfiguration ?? StaticCmsRuntimeConfiguration.Instance;
        _cmsDecisionEvaluator = new StrategyCmsDecisionEvaluator(_cmsRuntimeConfiguration);
        _reachabilityProbe = reachabilityProbe ?? NullStrategyReachabilityProbe.Instance;

        // Initialize factories
        _factories = new Dictionary<StrategyType, IStrategyFactory>
        {
                { StrategyType.SingleMetric, new SingleMetricStrategyFactory() },
                { StrategyType.CombinedMetric, new CombinedMetricStrategyFactory() },
                { StrategyType.MultiMetric, new MultiMetricStrategyFactory() },
                { StrategyType.Difference, new DifferenceStrategyFactory() },
                { StrategyType.Ratio, new RatioStrategyFactory() },
                { StrategyType.Normalized, new NormalizedStrategyFactory() },
                { StrategyType.WeeklyDistribution, new WeeklyDistributionStrategyFactory() },
                { StrategyType.WeekdayTrend, new WeekdayTrendStrategyFactory() },
                { StrategyType.HourlyDistribution, new HourlyDistributionStrategyFactory() }
        };
    }

    public bool ShouldUseCms(StrategyType strategyType, ChartDataContext ctx)
    {
        return _cmsDecisionEvaluator.Evaluate(strategyType, ctx).UseCms;
    }

    public IChartComputationStrategy CreateStrategy(StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        var decision = _cmsDecisionEvaluator.Evaluate(strategyType, ctx);
        Debug.WriteLine($"[CMS] {strategyType}: PrimarySamples={decision.PrimarySamples} (filtered), TotalPrimarySamples={_cmsDecisionEvaluator.GetSampleCount(ctx.PrimaryCms)}, SecondarySamples={decision.SecondarySamples} (filtered), TotalSecondarySamples={_cmsDecisionEvaluator.GetSampleCount(ctx.SecondaryCms)}");
        Debug.WriteLine($"[CutOver] Strategy={strategyType}, UseCms={decision.UseCms}, CmsRequested={decision.CmsRequested}, Reason={decision.Reason}, PrimaryCms={(ctx.PrimaryCms == null ? "NULL" : "SET")}, SecondaryCms={(ctx.SecondaryCms == null ? "NULL" : "SET")}, DateRange=[{ctx.From:yyyy-MM-dd} to {ctx.To:yyyy-MM-dd}]");
        _reachabilityProbe.Record(StrategyReachabilityRecord.Create(strategyType, ctx, decision));

        if (decision.UseCms)
            return CreateCmsStrategy(strategyType, ctx, parameters);

        return CreateLegacyStrategy(strategyType, parameters);
    }

    public ParityResult ValidateParity(IChartComputationStrategy legacyStrategy, IChartComputationStrategy cmsStrategy)
    {
        return _parityValidationService.ValidateParity(legacyStrategy, cmsStrategy);
    }

    public IChartComputationStrategy CreateCmsStrategy(StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        if (!_factories.TryGetValue(strategyType, out var factory))
            throw new NotSupportedException($"Strategy type {strategyType} is not supported");

        return factory.CreateCmsStrategy(ctx, parameters);
    }

    public IChartComputationStrategy CreateLegacyStrategy(StrategyType strategyType, StrategyCreationParameters parameters)
    {
        if (!_factories.TryGetValue(strategyType, out var factory))
            throw new NotSupportedException($"Strategy type {strategyType} is not supported");

        return factory.CreateLegacyStrategy(parameters);
    }

}
