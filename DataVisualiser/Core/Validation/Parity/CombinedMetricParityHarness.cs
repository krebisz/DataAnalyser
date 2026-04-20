// File: DataVisualiser/Charts/Parity/CombinedMetricParityHarness.cs

namespace DataVisualiser.Core.Validation.Parity;

/// <summary>
///     Parity harness for CombinedMetric strategies (Legacy vs CMS).
///     Disabled by default; invoked only when explicitly wired.
/// </summary>
public sealed class CombinedMetricParityHarness : IStrategyParityHarness
{
    public ParityResult Validate(StrategyParityContext context, Func<LegacyExecutionResult> legacyExecution, Func<CmsExecutionResult> cmsExecution)
    {
        var legacy = legacyExecution();
        var cms = cmsExecution();

        return ParitySeriesComparer.Compare(context, legacy.Series, cms.Series);
    }
}
