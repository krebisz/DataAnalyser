using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Reachability;

namespace DataVisualiser.Tests.Strategies;

public sealed class StrategyReachabilityStoreProbeTests
{
    [Fact]
    public void RecordAndClear_ShouldResetCapturedReachabilityRecords()
    {
        var probe = StrategyReachabilityStoreProbe.Default;
        probe.Clear();

        try
        {
            var context = new ChartDataContext
            {
                From = new DateTime(2026, 3, 26),
                To = new DateTime(2026, 3, 27),
                ActualSeriesCount = 2
            };

            var decision = new StrategyCmsDecision(false, true, false, false, true, 0, 0, "Test fallback");
            probe.Record(StrategyReachabilityRecord.Create(StrategyType.SingleMetric, context, decision));

            var snapshot = probe.Snapshot();
            Assert.Single(snapshot);
            Assert.Equal(StrategyType.SingleMetric, snapshot[0].StrategyType);

            probe.Clear();

            Assert.Empty(probe.Snapshot());
        }
        finally
        {
            probe.Clear();
        }
    }
}
