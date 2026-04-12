using DataVisualiser.Core.Rendering;
using DataVisualiser.Core.Rendering.Helpers;

namespace DataVisualiser.Tests.UI.Rendering;

public sealed class ChartSeriesLabelFormatterTests
{
    [Fact]
    public void FormatSeriesLabel_ShouldUseOperationFormat_ForPrimaryOperationSeries()
    {
        var model = new ChartRenderModel
        {
            MetricType = "Weight",
            PrimarySubtype = "fat_free_mass",
            SecondarySubtype = "body_fat_mass",
            OperationType = "-",
            IsOperationChart = true
        };

        var label = ChartSeriesLabelFormatter.FormatSeriesLabel(model, isPrimary: true, isSmoothed: true);

        Assert.Equal("Weight : fat_free_mass (-) Weight : body_fat_mass (smooth)", label);
    }

    [Fact]
    public void FormatSeriesLabel_ShouldOmitAllSubtype_ForIndependentSeries()
    {
        var model = new ChartRenderModel
        {
            PrimaryMetricType = "Weight",
            PrimarySubtype = "(All)"
        };

        var label = ChartSeriesLabelFormatter.FormatSeriesLabel(model, isPrimary: true, isSmoothed: false);

        Assert.Equal("Weight (raw)", label);
    }
}
