using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Validation;
using DataVisualiser.UI.MainHost;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class EvidenceStrategyParityExecutorTests
{
    [Fact]
    public void ExecuteSafe_WhenValidationSucceeds_ReturnsParitySnapshot()
    {
        var service = new FakeStrategyCutOverService
        {
            Result = new ParityResult
            {
                Passed = true,
                Message = "ok",
                Details = new Dictionary<string, object> { ["mode"] = "test" }
            }
        };

        var result = EvidenceStrategyParityExecutor.ExecuteSafe(
            service,
            StrategyType.SingleMetric,
            new ChartDataContext(),
            new StrategyCreationParameters());

        Assert.True(result.Passed);
        Assert.Equal("ok", result.Message);
        var details = Assert.IsType<Dictionary<string, object>>(result.Details);
        Assert.Equal("test", details["mode"]);
    }

    [Fact]
    public void ExecuteSafe_WhenServiceThrows_ReturnsErrorSnapshot()
    {
        var service = new FakeStrategyCutOverService
        {
            ThrowOnValidate = true
        };

        var result = EvidenceStrategyParityExecutor.ExecuteSafe(
            service,
            StrategyType.SingleMetric,
            new ChartDataContext(),
            new StrategyCreationParameters());

        Assert.False(result.Passed);
        Assert.Equal("boom", result.Error);
    }

    private sealed class FakeStrategyCutOverService : IStrategyCutOverService
    {
        public ParityResult Result { get; set; } = new();
        public bool ThrowOnValidate { get; set; }

        public IChartComputationStrategy CreateStrategy(StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters) => new FakeStrategy();
        public IChartComputationStrategy CreateCmsStrategy(StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters) => new FakeStrategy();
        public IChartComputationStrategy CreateLegacyStrategy(StrategyType strategyType, StrategyCreationParameters parameters) => new FakeStrategy();
        public bool ShouldUseCms(StrategyType strategyType, ChartDataContext ctx) => false;

        public ParityResult ValidateParity(IChartComputationStrategy legacyStrategy, IChartComputationStrategy cmsStrategy)
        {
            if (ThrowOnValidate)
                throw new InvalidOperationException("boom");

            return Result;
        }
    }

    private sealed class FakeStrategy : IChartComputationStrategy
    {
        public string PrimaryLabel => string.Empty;
        public string SecondaryLabel => string.Empty;
        public string? Unit => null;
        public ChartComputationResult? Compute() => null;
    }
}
