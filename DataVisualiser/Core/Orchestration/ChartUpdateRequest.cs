using DataVisualiser.Core.Computation.Results;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Core.Orchestration;

public sealed record ChartUpdateRequest
{
    public required string PrimaryLabel { get; init; }
    public string? SecondaryLabel { get; init; }
    public double MinHeight { get; init; } = 400.0;
    public string? MetricType { get; init; }
    public string? PrimarySubtype { get; init; }
    public string? SecondarySubtype { get; init; }
    public string? OperationType { get; init; }
    public bool IsOperationChart { get; init; }
    public string? SecondaryMetricType { get; init; }
    public string? DisplayPrimaryMetricType { get; init; }
    public string? DisplaySecondaryMetricType { get; init; }
    public string? DisplayPrimarySubtype { get; init; }
    public string? DisplaySecondarySubtype { get; init; }
    public bool IsStacked { get; init; }
    public bool IsCumulative { get; init; }
    public IReadOnlyList<SeriesResult>? OverlaySeries { get; init; }
    public ChartProgramKind RenderProgramKind { get; init; } = ChartProgramKind.Main;
    public ChartProgramRequest? RenderProgramRequest { get; init; }
    public CapabilityRequest? RenderCapability { get; init; }
    public ConsumerDeliveryContract? RenderDelivery { get; init; }
    public Func<ChartRenderPlan, VNextUiConsumptionContract>? RenderConsumptionContractFactory { get; init; }
}
