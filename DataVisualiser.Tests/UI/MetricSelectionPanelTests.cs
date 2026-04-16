using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.UI;

public sealed class MetricSelectionPanelTests
{
    [Fact]
    public void Panel_ShouldExposeAllRequiredControlProperties()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MetricSelectionPanel.xaml.cs");

        Assert.Contains("MetricTypeCombo", source);
        Assert.Contains("PrimarySubtypeCombo", source);
        Assert.Contains("FromDatePicker", source);
        Assert.Contains("ToDatePicker", source);
        Assert.Contains("ResolutionSelector", source);
        Assert.Contains("SubtypePanel", source);
        Assert.Contains("ThemeToggle", source);
        Assert.Contains("CmsEnable", source);
        Assert.Contains("CmsSingle", source);
        Assert.Contains("CmsCombined", source);
        Assert.Contains("CmsMulti", source);
        Assert.Contains("CmsNormalized", source);
        Assert.Contains("CmsWeekly", source);
        Assert.Contains("CmsWeekdayTrend", source);
        Assert.Contains("CmsHourly", source);
        Assert.Contains("CmsBarPie", source);
    }

    [Fact]
    public void Panel_ShouldExposeAllRequiredEvents()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MetricSelectionPanel.xaml.cs");

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

    [Fact]
    public void Panel_XamlShouldContainAllThreeControlRows()
    {
        var xaml = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MetricSelectionPanel.xaml");

        // Row 1: metric type, resolution, dates, buttons
        Assert.Contains("x:Name=\"ResolutionCombo\"", xaml);
        Assert.Contains("x:Name=\"TablesCombo\"", xaml);
        Assert.Contains("x:Name=\"FromDate\"", xaml);
        Assert.Contains("x:Name=\"ToDate\"", xaml);
        Assert.Contains("x:Name=\"ThemeToggleButton\"", xaml);

        // Row 2: subtype panel
        Assert.Contains("x:Name=\"SubtypeCombo\"", xaml);
        Assert.Contains("x:Name=\"TopControlMetricSubtypePanel\"", xaml);

        // Row 3: CMS toggles
        Assert.Contains("x:Name=\"CmsEnableCheckBox\"", xaml);
        Assert.Contains("x:Name=\"CmsBarPieCheckBox\"", xaml);
    }
}
