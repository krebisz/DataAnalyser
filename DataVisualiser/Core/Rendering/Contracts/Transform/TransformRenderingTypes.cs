using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Transform;

public enum TransformBackendKey
{
    LiveChartsWpfResultCartesian = 0
}

public enum TransformRenderingRoute
{
    ResultCartesian = 0
}

public enum TransformRenderingQualification
{
    Qualified = 0,
    TacticalFallback = 1,
    UnqualifiedDebt = 2
}

public sealed record TransformChartRenderRequest(
    TransformRenderingRoute Route,
    ChartDataContext Context,
    IChartComputationStrategy Strategy,
    string PrimaryLabel,
    string? OperationType,
    bool IsOperationChart,
    double MinHeight = 400.0,
    TransformCapabilityContract? CapabilityContract = null);

public sealed record TransformCapabilityContract : IAnalyticalCapabilityContract
{
    public TransformCapabilityContract(
        ChartProgramRequest programRequest,
        CapabilityRequest capability,
        ConsumerDeliveryContract delivery)
    {
        ArgumentNullException.ThrowIfNull(programRequest);
        ArgumentNullException.ThrowIfNull(capability);
        ArgumentNullException.ThrowIfNull(delivery);

        if (programRequest.Kind != ChartProgramKind.Transform)
            throw new ArgumentException("Transform capability contracts must use a Transform program request.", nameof(programRequest));
        if (delivery.ProgramKind != programRequest.Kind)
            throw new ArgumentException("Transform delivery contract must target the Transform program kind.", nameof(delivery));

        ProgramRequest = programRequest;
        Capability = capability;
        Delivery = delivery;
    }

    public ChartProgramRequest ProgramRequest { get; }
    public CapabilityRequest Capability { get; }
    public ConsumerDeliveryContract Delivery { get; }

    public static TransformCapabilityContract Create(
        string title,
        IReadOnlyList<SeriesOperationRequest>? operations = null)
    {
        var programRequest = ChartProgramRequest.Transform(
            string.IsNullOrWhiteSpace(title) ? "Transform" : title,
            operations ?? Array.Empty<SeriesOperationRequest>());
        return new TransformCapabilityContract(
            programRequest,
            CapabilityRequest.FromProgramRequest(programRequest),
            ChartProgramDeliveryTargetResolver.CreateDelivery(programRequest.Kind, "TransformChart"));
    }
}

public static class TransformVNextConsumptionContractBuilder
{
    public static VNextUiConsumptionContract Build(
        TransformChartRenderRequest request,
        ChartRenderPlan plan)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(plan);

        var capabilityContract = request.CapabilityContract
            ?? TransformCapabilityContract.Create(request.PrimaryLabel);
        var provider = ConsumerProviderContracts.LiveChartsWpf;

        return ChartRenderPlanConsumptionContractBuilder.Build(
            plan,
            capabilityContract,
            provider,
            new Dictionary<string, string>
            {
                ["Transform.Route"] = request.Route.ToString(),
                ["Transform.PrimaryLabel"] = request.PrimaryLabel,
                ["Transform.OperationType"] = request.OperationType ?? string.Empty,
                ["Transform.IsOperationChart"] = request.IsOperationChart.ToString()
            },
            "Transform");
    }
}

public sealed record TransformChartRenderHost(
    CartesianChart Chart,
    ChartState ChartState,
    Action? ResetAuxiliaryVisuals = null);
