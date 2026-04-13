using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.UI.MainHost;

internal static class EvidenceStrategyParityExecutor
{
    internal static ParityResultSnapshot ExecuteSafe(
        IStrategyCutOverService strategyCutOverService,
        StrategyType strategyType,
        ChartDataContext ctx,
        StrategyCreationParameters parameters)
    {
        try
        {
            var legacy = strategyCutOverService.CreateLegacyStrategy(strategyType, parameters);
            var cms = strategyCutOverService.CreateCmsStrategy(strategyType, ctx, parameters);
            var result = strategyCutOverService.ValidateParity(legacy, cms);
            return new ParityResultSnapshot
            {
                Passed = result.Passed,
                Message = result.Message,
                Details = result.Details
            };
        }
        catch (Exception ex)
        {
            return new ParityResultSnapshot
            {
                Passed = false,
                Error = ex.Message
            };
        }
    }
}
