using DataVisualiser.Core.Strategies.Reachability;

namespace DataVisualiser.UI.MainHost.Export;

public interface IReachabilityEvidenceStore
{
    IReadOnlyList<StrategyReachabilityRecord> Snapshot();
    void Clear();
}
