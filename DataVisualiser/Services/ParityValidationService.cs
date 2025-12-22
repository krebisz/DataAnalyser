using DataFileReader.Canonical;
using DataVisualiser.Charts;
using DataVisualiser.Charts.Parity;
using DataVisualiser.Charts.Strategies;
using DataVisualiser.Models;

namespace DataVisualiser.Services
{
    /// <summary>
    /// Service for executing parity validation between legacy and CMS strategies.
    /// Extracted from MainWindow to improve testability and modularity.
    /// </summary>
    public class ParityValidationService
    {
        /// <summary>
        /// Executes parity validation for CombinedMetric strategies if enabled.
        /// Returns the validated strategy (CMS if parity passes, legacy otherwise).
        /// </summary>
        public IChartComputationStrategy ExecuteCombinedMetricParityIfEnabled(
            ICanonicalMetricSeries? leftCms,
            ICanonicalMetricSeries? rightCms,
            IEnumerable<HealthMetricData> leftLegacy,
            IEnumerable<HealthMetricData> rightLegacy,
            string labelLeft,
            string labelRight,
            DateTime from,
            DateTime to,
            bool enableParity = false)
        {
            if (!enableParity || leftCms == null || rightCms == null)
            {
                return new CombinedMetricStrategy(leftLegacy, rightLegacy, labelLeft, labelRight, from, to);
            }

            var legacyStrategy = new CombinedMetricStrategy(leftLegacy, rightLegacy, labelLeft, labelRight, from, to);
            var cmsStrategy = new CombinedMetricCmsStrategy(leftCms, rightCms, labelLeft, labelRight, from, to);

            var harness = new CombinedMetricParityHarness();
            var parityResult = harness.Validate(
                new StrategyParityContext
                {
                    StrategyName = "CombinedMetric",
                    MetricIdentity = $"{labelLeft}|{labelRight}",
                    Mode = ParityMode.Diagnostic
                },
                legacyExecution: () => ParityResultAdapter.ToLegacyExecutionResult(legacyStrategy.Compute()),
                cmsExecution: () => ParityResultAdapter.ToCmsExecutionResult(cmsStrategy.Compute()));

            System.Diagnostics.Debug.WriteLine(
                parityResult.Passed
                    ? "[PARITY] CombinedMetric PASSED"
                    : "[PARITY] CombinedMetric FAILED");

            return cmsStrategy;
        }
    }
}
