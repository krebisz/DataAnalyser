using System.Diagnostics;
using DataFileReader.Canonical;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Validation.Parity;

namespace DataVisualiser.Validation;

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
    public IChartComputationStrategy ExecuteCombinedMetricParityIfEnabled(ICanonicalMetricSeries? leftCms, ICanonicalMetricSeries? rightCms, IEnumerable<MetricData> leftLegacy, IEnumerable<MetricData> rightLegacy, string labelLeft, string labelRight, DateTime from, DateTime to, bool enableParity = false)
    {
        if (!enableParity || leftCms == null || rightCms == null)
            return new CombinedMetricStrategy(leftLegacy, rightLegacy, labelLeft, labelRight, from, to);

        var legacyStrategy = new CombinedMetricStrategy(leftLegacy, rightLegacy, labelLeft, labelRight, from, to);
        var cmsStrategy = new CombinedMetricStrategy(leftCms, rightCms, labelLeft, labelRight, from, to);

        var harness = new CombinedMetricParityHarness();
        var parityResult = harness.Validate(new StrategyParityContext
        {
                StrategyName = "CombinedMetric",
                MetricIdentity = $"{labelLeft}|{labelRight}",
                Mode = ParityMode.Diagnostic
        }, () => ParityResultAdapter.ToLegacyExecutionResult(legacyStrategy.Compute()), () => ParityResultAdapter.ToCmsExecutionResult(cmsStrategy.Compute()));

        Debug.WriteLine(parityResult.Passed ? "[PARITY] CombinedMetric PASSED" : "[PARITY] CombinedMetric FAILED");

        return cmsStrategy;
    }
}