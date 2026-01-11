// File: DataVisualiser.Core.Validation/Parity/BucketDistributionParityHarness.cs

using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Validation.Parity;

/// <summary>
///     Base parity harness for bucket-based distribution strategies (Weekly, Hourly, etc.).
///     NOTE: Distribution ChartComputationResult uses empty timestamps, so parity must be based on ExtendedResult.
/// </summary>
public abstract class BucketDistributionParityHarness : IStrategyParityHarness
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

    // ---------- adapters (BucketDistributionResult -> execution result) ----------

    public LegacyExecutionResult ToLegacyExecutionResult(BucketDistributionResult? ext)
    {
        return new LegacyExecutionResult
        {
                Series = BuildParitySeries(ext)
        };
    }

    public CmsExecutionResult ToCmsExecutionResult(BucketDistributionResult? ext)
    {
        return new CmsExecutionResult
        {
                Series = BuildParitySeries(ext)
        };
    }

    private IReadOnlyList<ParitySeries> BuildParitySeries(BucketDistributionResult? ext)
    {
        if (ext == null)
            return Array.Empty<ParitySeries>();

        var baseDate = GetBaseDate();
        var timeAt = CreateTimeAtFunction(baseDate);
        var series = new List<ParitySeries>();

        AddArraySeries(series, "Mins", ext.Mins, timeAt);
        AddArraySeries(series, "Maxs", ext.Maxs, timeAt);
        AddArraySeries(series, "Ranges", ext.Ranges, timeAt);

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
                                Time = GetNextTime(baseDate),
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
                               })
                               .ToList()
        });
    }

    // ---------- abstract methods for derived classes ----------

    /// <summary>
    ///     Gets the base date for the synthetic timeline.
    /// </summary>
    protected abstract DateTime GetBaseDate();

    /// <summary>
    ///     Gets the next time point after the base date (for GlobalMinMax series).
    /// </summary>
    protected abstract DateTime GetNextTime(DateTime baseDate);

    /// <summary>
    ///     Creates a function that maps bucket index to DateTime for the synthetic timeline.
    /// </summary>
    protected abstract Func<int, DateTime> CreateTimeAtFunction(DateTime baseDate);

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
