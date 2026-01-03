namespace DataFileReader.Normalization.Canonical;

/// <summary>
///     Defines the canonical temporal model for a metric series.
/// </summary>
public sealed class TimeAxis
{
    public TimeAxis(bool isIntervalBased, TimeSpan resolution, TimeZoneInfo timeZone)
    {
        if (resolution <= TimeSpan.Zero)
            throw new ArgumentException("Resolution must be positive.", nameof(resolution));

        IsIntervalBased = isIntervalBased;
        Resolution = resolution;
        TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
    }

    public bool         IsIntervalBased { get; }
    public TimeSpan     Resolution      { get; }
    public TimeZoneInfo TimeZone        { get; }
}