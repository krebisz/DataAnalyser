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

        if (legacy.Series.Count != cms.Series.Count)
            return Fail(ParityLayer.StructuralParity, $"Series count mismatch: legacy={legacy.Series.Count}, cms={cms.Series.Count}", context);

        for (var i = 0; i < legacy.Series.Count; i++)
        {
            var l = legacy.Series[i];
            var c = cms.Series[i];

            if (l.Points.Count != c.Points.Count)
                return Fail(ParityLayer.TemporalParity, $"Point count mismatch in series '{l.SeriesKey}': legacy={l.Points.Count}, cms={c.Points.Count}", context);

            for (var p = 0; p < l.Points.Count; p++)
            {
                var lp = l.Points[p];
                var cp = c.Points[p];

                if (lp.Time != cp.Time)
                    return Fail(ParityLayer.TemporalParity, $"Timestamp mismatch at index {p} in series '{l.SeriesKey}': legacy={lp.Time:o}, cms={cp.Time:o}", context);

                if (!ValuesEqual(lp.Value, cp.Value, context))
                    return Fail(ParityLayer.ValueParity, $"Value mismatch at {lp.Time:o} in series '{l.SeriesKey}': legacy={lp.Value}, cms={cp.Value}", context);
            }
        }

        return ParityResult.Pass();
    }

    private static bool ValuesEqual(double a, double b, StrategyParityContext ctx)
    {
        if (double.IsNaN(a) && double.IsNaN(b))
            return true;

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