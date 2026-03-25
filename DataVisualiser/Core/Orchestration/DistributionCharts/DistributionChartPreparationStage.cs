using DataFileReader.Canonical;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Orchestration.DistributionCharts;

public sealed class DistributionChartPreparationStage : IDistributionChartPreparationStage
{
    private readonly IDistributionService _hourlyDistributionService;
    private readonly IDistributionService _weeklyDistributionService;

    public DistributionChartPreparationStage(
        IDistributionService weeklyDistributionService,
        IDistributionService hourlyDistributionService)
    {
        _weeklyDistributionService = weeklyDistributionService ?? throw new ArgumentNullException(nameof(weeklyDistributionService));
        _hourlyDistributionService = hourlyDistributionService ?? throw new ArgumentNullException(nameof(hourlyDistributionService));
    }

    public DistributionChartPreparedData Prepare(DistributionChartOrchestrationRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var context = request.Context;
        var data = context.Data1 ?? [];

        return new DistributionChartPreparedData(
            ResolveDistributionService(request.Mode),
            data,
            context.DisplayName1,
            context.From,
            context.To,
            request.ChartState.GetDistributionSettings(request.Mode),
            context.PrimaryCms as ICanonicalMetricSeries);
    }

    private IDistributionService ResolveDistributionService(DistributionMode mode)
    {
        return mode switch
        {
            DistributionMode.Weekly => _weeklyDistributionService,
            DistributionMode.Hourly => _hourlyDistributionService,
            _ => _weeklyDistributionService
        };
    }
}
