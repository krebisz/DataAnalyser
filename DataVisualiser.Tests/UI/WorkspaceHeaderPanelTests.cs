using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.UI;

public sealed class WorkspaceHeaderPanelTests
{
    [Fact]
    public void HeaderPanel_ShouldExposeReusableContentContainer()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "WorkspaceHeaderPanel.xaml.cs");

        Assert.Contains("ContentControl", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Template_ShouldOwnSharedHeaderChrome()
    {
        var xaml = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "WorkspaceHeaderPanel.xaml");

        Assert.Contains("ThemeTopBarBackgroundBrush", xaml, StringComparison.Ordinal);
        Assert.Contains("ThemePanelBorderBrush", xaml, StringComparison.Ordinal);
        Assert.Contains("CornerRadius=\"4\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Padding=\"8\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ContentPresenter", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void AppResources_ShouldLoadHeaderPanelBeforeWorkspaceHosts()
    {
        var appXaml = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "App.xaml");

        var headerIndex = appXaml.IndexOf("UI/WorkspaceHeaderPanel.xaml", StringComparison.Ordinal);
        var workspaceIndex = appXaml.IndexOf("UI/WorkspaceTabHost.xaml", StringComparison.Ordinal);

        Assert.True(headerIndex >= 0, "WorkspaceHeaderPanel resources should be merged into App.xaml.");
        Assert.True(workspaceIndex >= 0, "WorkspaceTabHost resources should be merged into App.xaml.");
        Assert.True(headerIndex < workspaceIndex, "WorkspaceHeaderPanel resources should load before workspace hosts use them.");
    }
}
