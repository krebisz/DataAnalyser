using System.Diagnostics;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Validation.Parity;

namespace DataVisualiser.Core.Validation;

/// <summary>
///     Service for executing parity validation between legacy and CMS strategies.
///     Extracted from MainWindow to improve testability and modularity.
/// </summary>
public class ParityValidationService
{
    /// <summary>
    ///     Executes parity validation for CombinedMetric strategies if enabled.
    ///     Returns the validated strategy (CMS if parity passes, legacy otherwise).
    /// </summary>
    public IChartComputationStrategy ExecuteCombinedMetricParityIfEnabled(IChartComputationStrategy legacyStrategy, IChartComputationStrategy? cmsStrategy, bool enableParity = false)
    {
        if (!enableParity || cmsStrategy == null)
            return legacyStrategy;

        var harness = new CombinedMetricParityHarness();
        var metricIdentity = $"{legacyStrategy.PrimaryLabel}|{legacyStrategy.SecondaryLabel}";
        var parityResult = harness.Validate(new StrategyParityContext
                {
                        StrategyName = "CombinedMetric",
                        MetricIdentity = metricIdentity,
                        Mode = ParityMode.Diagnostic
                },
                () => ParityResultAdapter.ToLegacyExecutionResult(legacyStrategy.Compute()),
                () => ParityResultAdapter.ToCmsExecutionResult(cmsStrategy.Compute()));

        Debug.WriteLine(parityResult.Passed ? "[PARITY] CombinedMetric PASSED" : "[PARITY] CombinedMetric FAILED");

        return cmsStrategy;
    }
}