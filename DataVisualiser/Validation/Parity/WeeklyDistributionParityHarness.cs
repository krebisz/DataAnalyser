// File: DataVisualiser/Charts/Parity/WeeklyDistributionParityHarness.cs

namespace DataVisualiser.Validation.Parity;

/// <summary>
///     Parity harness for WeeklyDistribution (Legacy vs CMS).
///     NOTE: WeeklyDistribution ChartComputationResult uses empty timestamps, so parity must be based on ExtendedResult.
/// </summary>
public sealed class WeeklyDistributionParityHarness : BucketDistributionParityHarness
{
    // ---------- abstract method implementations ----------

    protected override DateTime GetBaseDate()
    {
        return new DateTime(2000, 01, 03, 0, 0, 0, DateTimeKind.Unspecified); // Monday
    }

    protected override DateTime GetNextTime(DateTime baseDate)
    {
        return baseDate.AddDays(1);
    }

    protected override Func<int, DateTime> CreateTimeAtFunction(DateTime baseDate)
    {
        return idx => baseDate.AddDays(idx);
    }
}