namespace DataVisualiser.Core.Validation.Parity;

internal static class ParitySeriesComparer
{
    public static ParityResult Compare(
        StrategyParityContext context,
        IReadOnlyList<ParitySeries> legacySeries,
        IReadOnlyList<ParitySeries> cmsSeries,
        string timestampMismatchLabel = "Timestamp")
    {
        if (legacySeries.Count != cmsSeries.Count)
            return Fail(ParityLayer.StructuralParity, $"Series count mismatch: legacy={legacySeries.Count}, cms={cmsSeries.Count}", context);

        for (var i = 0; i < legacySeries.Count; i++)
        {
            var legacy = legacySeries[i];
            var cms = cmsSeries[i];

            if (legacy.Points.Count != cms.Points.Count)
                return Fail(ParityLayer.TemporalParity, $"Point count mismatch in series '{legacy.SeriesKey}': legacy={legacy.Points.Count}, cms={cms.Points.Count}", context);

            for (var p = 0; p < legacy.Points.Count; p++)
            {
                var legacyPoint = legacy.Points[p];
                var cmsPoint = cms.Points[p];

                if (legacyPoint.Time != cmsPoint.Time)
                    return Fail(ParityLayer.TemporalParity, $"{timestampMismatchLabel} mismatch at index {p} in series '{legacy.SeriesKey}': legacy={legacyPoint.Time:o}, cms={cmsPoint.Time:o}", context);

                if (!ValuesEqual(legacyPoint.Value, cmsPoint.Value, context))
                    return Fail(ParityLayer.ValueParity, $"Value mismatch at {legacyPoint.Time:o} in series '{legacy.SeriesKey}': legacy={legacyPoint.Value}, cms={cmsPoint.Value}", context);
            }
        }

        return ParityResult.Pass();
    }

    private static bool ValuesEqual(double a, double b, StrategyParityContext context)
    {
        if (double.IsNaN(a) && double.IsNaN(b))
            return true;

        if (!context.Tolerance.AllowFloatingPointDrift)
            return a.Equals(b);

        return Math.Abs(a - b) <= context.Tolerance.ValueEpsilon;
    }

    private static ParityResult Fail(ParityLayer layer, string message, StrategyParityContext context)
    {
        var failure = new ParityFailure
        {
            Layer = layer,
            Message = message
        };

        if (context.Mode == ParityMode.Strict)
            throw new InvalidOperationException($"Parity failure [{layer}]: {message}");

        return ParityResult.Fail(failure);
    }
}
