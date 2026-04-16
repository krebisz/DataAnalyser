using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.UI;

public sealed class ChartTabHostTests
{
    [Fact]
    public void Host_ShouldOwnSelectionSurfaceAndScrollableChartContentSlot()
    {
        var xaml = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "ChartTabHost.xaml");

        Assert.Contains("MetricSelectionPanel", xaml);
        Assert.Contains("x:Name=\"PART_SelectionPanel\"", xaml);
        Assert.Contains("ScrollViewer", xaml);
        Assert.Contains("ContentPresenter", xaml);
        Assert.Contains("ChartContent", xaml);
    }

    [Fact]
    public void Host_ShouldExposeSelectionSurfaceAndChartContent()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "ChartTabHost.xaml.cs");

        Assert.Contains("DependencyProperty.Register", source);
        Assert.Contains("ChartContentProperty", source);
        Assert.Contains("SelectionSurface", source);
        Assert.Contains("ContentProperty", source);
    }
}
