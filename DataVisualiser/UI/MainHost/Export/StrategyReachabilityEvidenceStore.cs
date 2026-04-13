using DataVisualiser.Core.Strategies.Reachability;

namespace DataVisualiser.UI.MainHost.Export;

public sealed class StrategyReachabilityEvidenceStore : IReachabilityEvidenceStore
{
    public IReadOnlyList<StrategyReachabilityRecord> Snapshot()
    {
        return StrategyReachabilityStoreProbe.Default.Snapshot();
    }

    public void Clear()
    {
        StrategyReachabilityStoreProbe.Default.Clear();
    }
}
