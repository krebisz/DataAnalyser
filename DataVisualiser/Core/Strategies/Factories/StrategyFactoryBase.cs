using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Base class for strategy factories that reduces duplication.
///     Factories can override CreateCmsStrategy and CreateLegacyStrategy to provide custom logic,
///     or use the delegate-based constructors for simple cases.
/// </summary>
public abstract class StrategyFactoryBase : IStrategyFactory
{
    private readonly Func<ChartDataContext, StrategyCreationParameters, IChartComputationStrategy>? _cmsFactory;
    private readonly Func<StrategyCreationParameters, IChartComputationStrategy>?                   _legacyFactory;

    /// <summary>
    ///     Constructor for factories that need custom logic (override methods).
    /// </summary>
    protected StrategyFactoryBase()
    {
    }

    /// <summary>
    ///     Constructor for factories with simple delegate-based creation.
    /// </summary>
    protected StrategyFactoryBase(Func<ChartDataContext, StrategyCreationParameters, IChartComputationStrategy>? cmsFactory, Func<StrategyCreationParameters, IChartComputationStrategy>? legacyFactory)
    {
        _cmsFactory = cmsFactory;
        _legacyFactory = legacyFactory;
    }

    /// <summary>
    ///     Creates a CMS strategy. Override this method or provide a delegate in the constructor.
    /// </summary>
    public virtual IChartComputationStrategy CreateCmsStrategy(ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        if (_cmsFactory != null)
            return _cmsFactory(ctx, parameters);

        throw new NotImplementedException($"CreateCmsStrategy is not implemented for {GetType().Name}");
    }

    /// <summary>
    ///     Creates a legacy strategy. Override this method or provide a delegate in the constructor.
    /// </summary>
    public virtual IChartComputationStrategy CreateLegacyStrategy(StrategyCreationParameters parameters)
    {
        if (_legacyFactory != null)
            return _legacyFactory(parameters);

        throw new NotImplementedException($"CreateLegacyStrategy is not implemented for {GetType().Name}");
    }
}