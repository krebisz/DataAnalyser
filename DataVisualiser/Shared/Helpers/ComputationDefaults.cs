namespace DataVisualiser.Shared.Helpers;

/// <summary>
///     Centralized defaults for computation behavior and policies.
/// </summary>
public static class ComputationDefaults
{
    public const int SmoothingWindow = 3;

    public const double ForwardFillSeedValue = 0.0;

    public const double RatioDivideByZeroValue = 0.0;

    public const double SqlLimitingMinDaysNoLimit = 1.0;
    public const double SqlLimitingMaxDailyDaysNoLimit = 730.0;
    public const double SqlLimitingMaxHourlyDaysNoLimit = 365.0;
    public const double SqlLimitingEstimatedRecordsPerDay = 24.0;
    public const int SqlLimitingMaxRecords = 10000;
}
