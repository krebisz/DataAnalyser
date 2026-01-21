namespace DataVisualiser.Core.Strategies.Reachability;

public sealed class NullStrategyReachabilityProbe : IStrategyReachabilityProbe
{
    public static readonly NullStrategyReachabilityProbe Instance = new();

    private NullStrategyReachabilityProbe()
    {
    }

    public void Record(StrategyReachabilityRecord record)
    {
    }
}
