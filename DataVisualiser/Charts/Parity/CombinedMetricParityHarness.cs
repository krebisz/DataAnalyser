// File: DataVisualiser/Charts/Parity/CombinedMetricParityHarness.cs

using System;
using System.Collections.Generic;
using System.Linq;
using DataVisualiser.Charts.Computation;
using DataVisualiser.Charts.Parity;

namespace DataVisualiser.Charts.Parity
{
    /// <summary>
    /// Parity harness for CombinedMetric strategies (Legacy vs CMS).
    /// Disabled by default; invoked only when explicitly wired.
    /// </summary>
    public sealed class CombinedMetricParityHarness : IStrategyParityHarness
    {
        public ParityResult Validate(
            StrategyParityContext context,
            Func<LegacyExecutionResult> legacyExecution,
            Func<CmsExecutionResult> cmsExecution)
        {
            var legacy = legacyExecution();
            var cms = cmsExecution();

            // --- Structural parity ---
            if (legacy.Series.Count != cms.Series.Count)
            {
                return Fail(
                    ParityLayer.StructuralParity,
                    $"Series count mismatch: legacy={legacy.Series.Count}, cms={cms.Series.Count}",
                    context);
            }

            for (int i = 0; i < legacy.Series.Count; i++)
            {
                var l = legacy.Series[i];
                var c = cms.Series[i];

                // --- Temporal parity ---
                if (l.Points.Count != c.Points.Count)
                {
                    return Fail(
                        ParityLayer.TemporalParity,
                        $"Point count mismatch in series '{l.SeriesKey}': legacy={l.Points.Count}, cms={c.Points.Count}",
                        context);
                }

                for (int p = 0; p < l.Points.Count; p++)
                {
                    var lp = l.Points[p];
                    var cp = c.Points[p];

                    if (lp.Time != cp.Time)
                    {
                        return Fail(
                            ParityLayer.TemporalParity,
                            $"Timestamp mismatch at index {p} in series '{l.SeriesKey}': legacy={lp.Time:o}, cms={cp.Time:o}",
                            context);
                    }

                    // --- Value parity ---
                    if (!ValuesEqual(lp.Value, cp.Value, context))
                    {
                        return Fail(
                            ParityLayer.ValueParity,
                            $"Value mismatch at {lp.Time:o} in series '{l.SeriesKey}': legacy={lp.Value}, cms={cp.Value}",
                            context);
                    }
                }
            }

            return ParityResult.Pass();
        }

        // ---------- helpers ----------

        private static bool ValuesEqual(double a, double b, StrategyParityContext ctx)
        {
            if (double.IsNaN(a) && double.IsNaN(b)) return true;

            if (!ctx.Tolerance.AllowFloatingPointDrift)
                return a.Equals(b);

            return Math.Abs(a - b) <= ctx.Tolerance.ValueEpsilon;
        }

        private static ParityResult Fail(ParityLayer layer, string message, StrategyParityContext ctx)
        {
            var failure = new ParityFailure
            {
                Layer = layer,
                Message = message
            };

            if (ctx.Mode == ParityMode.Strict)
                throw new InvalidOperationException($"Parity failure [{layer}]: {message}");

            return ParityResult.Fail(failure);
        }
    }
}
