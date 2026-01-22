using System.Collections.Concurrent;
using System.Diagnostics;

namespace DataVisualiser.Core.Strategies.Reachability;

public sealed class StrategyReachabilityStoreProbe : IStrategyReachabilityProbe
{
    public static readonly StrategyReachabilityStoreProbe Default = new();

    private readonly ConcurrentQueue<StrategyReachabilityRecord> _records = new();

    private StrategyReachabilityStoreProbe()
    {
    }

    public void Record(StrategyReachabilityRecord record)
    {
        _records.Enqueue(record);
        Debug.WriteLine($"[Reachability] Strategy={record.StrategyType}, UsedCms={record.UsedCms}, PrimaryCms={record.PrimaryCmsAvailable}, SecondaryCms={record.SecondaryCmsAvailable}, PrimarySamples={record.PrimarySamples}, SecondarySamples={record.SecondarySamples}, Range=[{record.From:yyyy-MM-dd} to {record.To:yyyy-MM-dd}]");
    }

    public IReadOnlyList<StrategyReachabilityRecord> Snapshot()
    {
        return _records.ToArray();
    }

    public void Clear()
    {
        while (_records.TryDequeue(out _))
        {
        }
    }
}