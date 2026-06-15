using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class TransformMetricSelectionRequestFactoryTests
{
    [Fact]
    public void Create_ShouldUseSingleMetricTypeWhenAllInputsMatch()
    {
        var request = TransformMetricSelectionRequestFactory.Create(
            [
                new MetricSeriesRequest("Weight", "fat"),
                new MetricSeriesRequest("Weight", "lean")
            ],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");

        Assert.Equal("Weight", request.MetricType);
        Assert.Equal(2, request.Series.Count);
    }

    [Fact]
    public void Create_ShouldUseMixedMetricTypeForIndependentInputs()
    {
        var request = TransformMetricSelectionRequestFactory.Create(
            [
                new MetricSeriesRequest("Weight", "fat"),
                new MetricSeriesRequest("Steps", "count")
            ],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");

        Assert.Equal("Mixed", request.MetricType);
    }
}
