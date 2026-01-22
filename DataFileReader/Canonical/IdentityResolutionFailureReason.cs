namespace DataFileReader.Canonical;

/// <summary>
///     Reasons for identity resolution failure.
///     Phase 4: Made public for DataVisualiser integration.
/// </summary>
public enum IdentityResolutionFailureReason
{
    Unknown,
    NoMatchingRule,
    MultipleMatchingRules,
    MissingRequiredMetadata,
    InvalidMetadata
}