using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MetricSelectionPanelEventBinderTests
{
    [Fact]
    public void Binder_ShouldWireAllSelectionPanelEvents()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainHost", "Coordination", "MetricSelectionPanelEventBinder.cs");

        Assert.Contains("LoadDataRequested", source);
        Assert.Contains("ResetZoomRequested", source);
        Assert.Contains("ClearRequested", source);
        Assert.Contains("ExportReachabilityRequested", source);
        Assert.Contains("ThemeToggleRequested", source);
        Assert.Contains("AddSubtypeRequested", source);
        Assert.Contains("ResolutionSelectionChanged", source);
        Assert.Contains("MetricTypeSelectionChanged", source);
        Assert.Contains("FromDateChanged", source);
        Assert.Contains("ToDateChanged", source);
        Assert.Contains("CmsToggleChanged", source);
        Assert.Contains("CmsStrategyToggled", source);
    }
}
