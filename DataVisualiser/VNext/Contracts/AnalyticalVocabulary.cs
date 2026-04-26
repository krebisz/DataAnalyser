namespace DataVisualiser.VNext.Contracts;

public enum ConsumerKind
{
    Chart,
    HierarchyChart,
    Export,
    Api,
    Other
}

public enum OverlayKind
{
    ReferenceLine,
    AverageLine,
    MedianLine,
    Threshold,
    ConfidenceMarker
}

public enum InteractionKind
{
    VisibilityToggle,
    ResetZoom,
    ViewportChange,
    Selection,
    Hover
}

public enum AnalyticalCapabilityKind
{
    Identity,
    Normalization,
    Comparison,
    Transform,
    Distribution,
    TemporalTrend,
    Hierarchy
}

public enum CompositionKind
{
    SingleSeries,
    MultiSeries,
    DerivedSeries,
    Hierarchy
}

public enum AnalyticalAuthority
{
    Legacy,
    VNext,
    User,
    External
}

public enum ProvenanceTrustClass
{
    Raw,
    Requested,
    Normalized,
    Canonical,
    Derived,
    Projected,
    Delivered
}
