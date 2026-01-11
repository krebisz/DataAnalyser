namespace DataVisualiser.Core.Validation;

/// <summary>
///     Result of parity validation.
/// </summary>
public class ParityResult
{
    public bool Passed { get; init; }
    public string? Message { get; init; }
    public Dictionary<string, object>? Details { get; init; }
}
