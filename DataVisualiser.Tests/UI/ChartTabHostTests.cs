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
        Assert.Contains("WorkspaceTabHost", xaml);
        Assert.Contains("HeaderContent", xaml);
        Assert.Contains("ChartContent", xaml);
        Assert.DoesNotContain("<DockPanel", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<ScrollViewer", xaml, StringComparison.Ordinal);
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
