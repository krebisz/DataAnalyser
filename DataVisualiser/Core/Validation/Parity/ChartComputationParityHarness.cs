namespace DataVisualiser.Core.Validation.Parity;

/// <summary>
///     Generic parity harness for ChartComputationResult-based strategies.
///     Compares series counts, timestamps, and values with configured tolerance.
/// </summary>
public sealed class ChartComputationParityHarness : IStrategyParityHarness
{
    public ParityResult Validate(StrategyParityContext context, Func<LegacyExecutionResult> legacyExecution, Func<CmsExecutionResult> cmsExecution)
    {
        var legacy = legacyExecution();
        var cms = cmsExecution();

        return ParitySeriesComparer.Compare(context, legacy.Series, cms.Series);
    }
}
