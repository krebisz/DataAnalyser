namespace DataFileReader.Canonical;

/// <summary>
///     Declares how time is represented for the series.
/// </summary>
public sealed record TimeSemantics(TimeRepresentation Representation, DateTimeOffset Start, DateTimeOffset? End);