// File: DataVisualiser/Charts/Parity/HourlyDistributionParityHarness.cs

namespace DataVisualiser.Core.Validation.Parity;

/// <summary>
///     Parity harness for HourlyDistribution (Legacy vs CMS).
///     NOTE: HourlyDistribution ChartComputationResult uses empty timestamps, so parity must be based on ExtendedResult.
/// </summary>
public sealed class HourlyDistributionParityHarness : BucketDistributionParityHarness
{
    // ---------- abstract method implementations ----------

    protected override DateTime GetBaseDate()
    {
        return new DateTime(2000, 01, 03, 0, 0, 0, DateTimeKind.Unspecified);
    }

    protected override DateTime GetNextTime(DateTime baseDate)
    {
        return baseDate.AddHours(1);
    }

    protected override Func<int, DateTime> CreateTimeAtFunction(DateTime baseDate)
    {
        return idx => baseDate.AddHours(idx);
    }
}