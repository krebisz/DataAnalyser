using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.UI;

public sealed class WorkspaceTabHostTests
{
    [Fact]
    public void Host_ShouldExposeGenericHeaderAndBodyContentSlots()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "WorkspaceTabHost.xaml.cs");

        Assert.Contains("HeaderContentProperty", source);
        Assert.Contains("BodyContentProperty", source);
        Assert.Contains("ContentProperty", source);
    }

    [Fact]
    public void Template_ShouldOwnSharedTabLayoutWithoutMetricSpecificControls()
    {
        var xaml = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "WorkspaceTabHost.xaml");

        Assert.Contains("DockPanel", xaml);
        Assert.Contains("ScrollViewer", xaml);
        Assert.Contains("HeaderContent", xaml);
        Assert.Contains("BodyContent", xaml);
        Assert.DoesNotContain("MetricSelectionPanel", xaml);
    }

    [Fact]
    public void AppResources_ShouldLoadWorkspaceHostBeforeChartHostSpecialization()
    {
        var appXaml = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "App.xaml");

        var workspaceIndex = appXaml.IndexOf("UI/WorkspaceTabHost.xaml", StringComparison.Ordinal);
        var chartIndex = appXaml.IndexOf("UI/ChartTabHost.xaml", StringComparison.Ordinal);

        Assert.True(workspaceIndex >= 0, "WorkspaceTabHost resources should be merged into App.xaml.");
        Assert.True(chartIndex >= 0, "ChartTabHost resources should be merged into App.xaml.");
        Assert.True(workspaceIndex < chartIndex, "WorkspaceTabHost resources should be loaded before ChartTabHost resources.");
    }
}
