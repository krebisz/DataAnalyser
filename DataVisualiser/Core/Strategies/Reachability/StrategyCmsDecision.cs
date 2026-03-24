namespace DataVisualiser.Core.Strategies.Reachability;

public sealed record StrategyCmsDecision(
    bool UseCms,
    bool CmsRequested,
    bool GlobalCmsEnabled,
    bool StrategyCmsEnabled,
    bool RealCmsSupported,
    int PrimarySamples,
    int SecondarySamples,
    string Reason);
