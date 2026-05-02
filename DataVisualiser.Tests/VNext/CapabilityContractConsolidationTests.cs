using DataVisualiser.Core.Rendering.BarPie;
using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Core.Rendering.Distribution;
using DataVisualiser.Core.Rendering.Syncfusion;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Rendering.WeekdayTrend;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Tests.VNext;

public sealed class CapabilityContractConsolidationTests
{
    [Fact]
    public void CapabilityContracts_ShouldShareCommonContractShapeWithoutFlatteningFamilyDifferences()
    {
        IAnalyticalCapabilityContract[] contracts =
        [
            DistributionCapabilityContract.Create(),
            WeekdayTrendCapabilityContract.Create(),
            BarPieCapabilityContract.Create(),
            TransformCapabilityContract.Create("Transform", [SeriesOperationRequest.Normalize(0, "norm", "Normalized")]),
            SyncfusionSunburstCapabilityContract.Create(),
            CartesianMetricCapabilityContract.Create(ChartProgramKind.Main),
            CartesianMetricCapabilityContract.Create(ChartProgramKind.Normalized),
            MovingAverageCapabilityContract.Create("Moving Average", [SeriesOperationRequest.MovingAverage(0, 3, "ma", "MA")])
        ];

        Assert.All(contracts, contract =>
        {
            Assert.Equal(contract.ProgramRequest.Kind, contract.Delivery.ProgramKind);
            Assert.Equal(CapabilityRequest.FromProgramRequest(contract.ProgramRequest).Signature, contract.Capability.Signature);
        });
        Assert.Contains(contracts, contract => contract.Delivery.ConsumerKind == ConsumerKind.HierarchyChart);
        Assert.Contains(contracts, contract => contract.Capability.CompositionKind == CompositionKind.DerivedSeries);
        Assert.Contains(contracts, contract => contract.Capability.CapabilityKind == AnalyticalCapabilityKind.Distribution);
        Assert.Contains(contracts, contract => contract.Capability.CapabilityKind == AnalyticalCapabilityKind.TemporalTrend);
        Assert.Contains(contracts, contract => contract.ProgramRequest.Kind == ChartProgramKind.Normalized);
        Assert.Equal(contracts.Length, contracts.Select(contract => contract.ProgramRequest.Kind).Distinct().Count());
    }
}
