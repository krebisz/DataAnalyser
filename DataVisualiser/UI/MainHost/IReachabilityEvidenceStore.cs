using DataVisualiser.Core.Strategies.Reachability;

namespace DataVisualiser.UI.MainHost;

public interface IReachabilityEvidenceStore
{
    IReadOnlyList<StrategyReachabilityRecord> Snapshot();
    void Clear();
}
