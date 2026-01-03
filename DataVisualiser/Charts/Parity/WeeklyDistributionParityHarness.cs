// File: DataVisualiser/Charts/Parity/WeeklyDistributionParityHarness.cs

using DataVisualiser.Models;

namespace DataVisualiser.Charts.Parity;

/// <summary>
///     Parity harness for WeeklyDistribution (Legacy vs CMS).
///     NOTE: WeeklyDistribution ChartComputationResult uses empty timestamps, so parity must be based on ExtendedResult.
/// </summary>
public sealed class WeeklyDistributionParityHarness : IStrategyParityHarness
{
    public ParityResult Validate(StrategyParityContext context, Func<LegacyExecutionResult> legacyExecution, Func<CmsExecutionResult> cmsExecution)
    {
        var legacy = legacyExecution();
        var cms = cmsExecution();

        // --- Structural parity ---
        if (legacy.Series.Count != cms.Series.Count)
            return Fail(ParityLayer.StructuralParity, $"Series count mismatch: legacy={legacy.Series.Count}, cms={cms.Series.Count}", context);

        for (var i = 0; i < legacy.Series.Count; i++)
        {
            var l = legacy.Series[i];
            var c = cms.Series[i];

            // --- Temporal parity ---
            if (l.Points.Count != c.Points.Count)
                return Fail(ParityLayer.TemporalParity, $"Point count mismatch in series '{l.SeriesKey}': legacy={l.Points.Count}, cms={c.Points.Count}", context);

            for (var p = 0; p < l.Points.Count; p++)
            {
                var lp = l.Points[p];
                var cp = c.Points[p];

                // --- Temporal parity ---
                if (lp.Time != cp.Time)
                    return Fail(ParityLayer.TemporalParity, $"Time mismatch at index {p} in series '{l.SeriesKey}': legacy={lp.Time:o}, cms={cp.Time:o}", context);

                // --- Value parity ---
                if (!ValuesEqual(lp.Value, cp.Value, context))
                    return Fail(ParityLayer.ValueParity, $"Value mismatch at {lp.Time:o} in series '{l.SeriesKey}': legacy={lp.Value}, cms={cp.Value}", context);
            }
        }

        return ParityResult.Pass();
    }

    // ---------- adapters (WeeklyDistributionResult -> execution result) ----------

    public static LegacyExecutionResult ToLegacyExecutionResult(WeeklyDistributionResult? ext)
    {
        return new LegacyExecutionResult
        {
            Series = BuildParitySeries(ext)
        };
    }

    public static CmsExecutionResult ToCmsExecutionResult(WeeklyDistributionResult? ext)
    {
        return new CmsExecutionResult
        {
            Series = BuildParitySeries(ext)
        };
    }

    private static IReadOnlyList<ParitySeries> BuildParitySeries(WeeklyDistributionResult? ext)
    {
        if (ext == null)
            return Array.Empty<ParitySeries>();

        // Synthetic "week index" timeline (Mon..Sun)
        var baseDate = new DateTime(2000, 01, 03, 0, 0, 0, DateTimeKind.Unspecified); // Monday

        DateTime T(int idx)
        {
            return baseDate.AddDays(idx);
        }

        var series = new List<ParitySeries>();

        AddArraySeries(series, "Mins", ext.Mins, T);
        AddArraySeries(series, "Maxs", ext.Maxs, T);
        AddArraySeries(series, "Ranges", ext.Ranges, T);

        series.Add(new ParitySeries
        {
            SeriesKey = "GlobalMinMax",
            Points = new List<ParityPoint>
            {
                new()
                {
                    Time = baseDate,
                    Value = ext.GlobalMin
                },
                new()
                {
                    Time = baseDate.AddDays(1),
                    Value = ext.GlobalMax
                }
            }
        });

        return series;
    }

    private static void AddArraySeries(List<ParitySeries> series, string key, IReadOnlyList<double>? values, Func<int, DateTime> timeAt)
    {
        if (values == null)
            return;

        series.Add(new ParitySeries
        {
            SeriesKey = key,
            Points = values.Select((v, i) => new ParityPoint
                {
                    Time = timeAt(i),
                    Value = v
                }).
                ToList()
        });
    }

    // ---------- helpers ----------

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