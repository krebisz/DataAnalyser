using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Core.Strategies.Reachability;

public sealed record StrategyReachabilityRecord(
    StrategyType StrategyType,
    bool UsedCms,
    bool CmsRequested,
    bool GlobalCmsEnabled,
    bool StrategyCmsEnabled,
    bool RealCmsSupported,
    string DecisionReason,
    bool PrimaryCmsAvailable,
    bool SecondaryCmsAvailable,
    int PrimarySamples,
    int SecondarySamples,
    int CmsSeriesCount,
    int ActualSeriesCount,
    DateTime From,
    DateTime To,
    DateTime TimestampUtc)
{
    public static StrategyReachabilityRecord Create(StrategyType strategyType, ChartDataContext ctx, StrategyCmsDecision decision)
    {
        return new StrategyReachabilityRecord(
            strategyType,
            decision.UseCms,
            decision.CmsRequested,
            decision.GlobalCmsEnabled,
            decision.StrategyCmsEnabled,
            decision.RealCmsSupported,
            decision.Reason,
            ctx.PrimaryCms != null,
            ctx.SecondaryCms != null,
            decision.PrimarySamples,
            decision.SecondarySamples,
            ctx.CmsSeries?.Count ?? 0,
            ctx.ActualSeriesCount,
            ctx.From,
            ctx.To,
            DateTime.UtcNow);
    }
}
