namespace DataVisualiser.VNext.Contracts;

public interface IAnalyticalCapabilityContract
{
    ChartProgramRequest ProgramRequest { get; }
    CapabilityRequest Capability { get; }
    ConsumerDeliveryContract Delivery { get; }
}
