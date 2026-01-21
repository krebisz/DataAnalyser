using System;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Core.Strategies.Reachability;

public sealed record StrategyReachabilityRecord(
    StrategyType StrategyType,
    bool UsedCms,
    bool PrimaryCmsAvailable,
    bool SecondaryCmsAvailable,
    int PrimarySamples,
    int SecondarySamples,
    DateTime From,
    DateTime To,
    DateTime TimestampUtc)
{
    public static StrategyReachabilityRecord Create(StrategyType strategyType, bool usedCms, ChartDataContext ctx, int primarySamples, int secondarySamples)
    {
        return new StrategyReachabilityRecord(
            strategyType,
            usedCms,
            ctx.PrimaryCms != null,
            ctx.SecondaryCms != null,
            primarySamples,
            secondarySamples,
            ctx.From,
            ctx.To,
            DateTime.UtcNow);
    }
}
