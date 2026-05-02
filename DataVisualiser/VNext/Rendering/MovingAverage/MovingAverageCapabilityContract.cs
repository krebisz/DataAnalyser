using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Rendering.MovingAverage;

public sealed record MovingAverageCapabilityContract : IAnalyticalCapabilityContract
{
    public MovingAverageCapabilityContract(
        ChartProgramRequest programRequest,
        CapabilityRequest capability,
        ConsumerDeliveryContract delivery)
    {
        ArgumentNullException.ThrowIfNull(programRequest);
        ArgumentNullException.ThrowIfNull(capability);
        ArgumentNullException.ThrowIfNull(delivery);

        if (programRequest.Kind != ChartProgramKind.MovingAverage)
            throw new ArgumentException("MovingAverage capability contracts must use a MovingAverage program request.", nameof(programRequest));
        if (delivery.ProgramKind != programRequest.Kind)
            throw new ArgumentException("MovingAverage delivery contract must target the MovingAverage program kind.", nameof(delivery));

        ProgramRequest = programRequest;
        Capability = capability;
        Delivery = delivery;
    }

    public ChartProgramRequest ProgramRequest { get; }
    public CapabilityRequest Capability { get; }
    public ConsumerDeliveryContract Delivery { get; }

    public static MovingAverageCapabilityContract Create(
        string title,
        IReadOnlyList<SeriesOperationRequest>? operations = null)
    {
        var programRequest = ChartProgramRequest.MovingAverage(
            string.IsNullOrWhiteSpace(title) ? "Moving Average" : title,
            operations ?? []);
        return new MovingAverageCapabilityContract(
            programRequest,
            CapabilityRequest.FromProgramRequest(programRequest),
            ChartProgramDeliveryTargetResolver.CreateDelivery(programRequest.Kind));
    }
}
