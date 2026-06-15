using DataVisualiser.Tests.Helpers.Infrastructure;

namespace DataVisualiser.Tests.UI.Syncfusion;

public sealed class SyncfusionSunburstChartControllerSourceTests
{
    [Fact]
    public void RingLabels_ShouldUseRingNumbersAndExposeDateIntervalLegend()
    {
        var xaml = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "UI",
            "Charts",
            "Controllers",
            "SyncfusionSunburstChartController.xaml");
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "UI",
            "Charts",
            "Controllers",
            "SyncfusionSunburstChartController.xaml.cs");

        Assert.Contains("x:Name=\"RingLegendPanel\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Grid.Column=\"1\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Orientation=\"Vertical\"", xaml, StringComparison.Ordinal);
        Assert.Contains("RingLegendPanel.Visibility", source, StringComparison.Ordinal);
        Assert.Contains("UpdateRingLegend();", source, StringComparison.Ordinal);
        Assert.Contains("var label = (i + 1).ToString(CultureInfo.InvariantCulture);", source, StringComparison.Ordinal);
        Assert.Contains("Text = $\"{index + 1} : {periodLabel}\"", source, StringComparison.Ordinal);
        Assert.Contains("TextWrapping = TextWrapping.Wrap", source, StringComparison.Ordinal);
    }
}
