using DataVisualiser.Tests.Helpers.Infrastructure;

namespace DataVisualiser.Tests.UI.OperationChain;

public sealed class OperationChainWorkbenchViewXamlTests
{
    [Fact]
    public void OutputChart_ShouldExposeSharedZoomPanAndResetZoomControls()
    {
        var xaml = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "UI",
            "OperationChain",
            "OperationChainWorkbenchView.xaml");

        Assert.Contains("x:Name=\"OutputChartResetZoomButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"Reset Zoom\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"OnOutputChartResetZoomClicked\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Zoom=\"{x:Static ui:ChartUiDefaults.DefaultZoom}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Pan=\"{x:Static ui:ChartUiDefaults.DefaultPan}\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void WorkbenchStartup_ShouldCreateOnlyOneInputRow()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "UI",
            "OperationChain",
            "OperationChainWorkbenchView.xaml.cs");

        Assert.Contains("EnsureInputRowCount(1);", source, StringComparison.Ordinal);
        Assert.DoesNotContain("EnsureInputRowCount(3);", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ClearButton_ShouldResetControlSelectionsToStartupDefaults()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "UI",
            "OperationChain",
            "OperationChainWorkbenchView.xaml.cs");

        Assert.Contains("await ResetControlSelectionsToDefaultsAsync();", source, StringComparison.Ordinal);
        Assert.Contains("ResetInputRowsToInitialState();", source, StringComparison.Ordinal);
        Assert.Contains("ResolutionCombo.SelectedItem = \"All\";", source, StringComparison.Ordinal);
        Assert.Contains("FromDate.SelectedDate = null;", source, StringComparison.Ordinal);
        Assert.Contains("ToDate.SelectedDate = null;", source, StringComparison.Ordinal);
        Assert.Contains("OperationCombo.SelectedIndex = 0;", source, StringComparison.Ordinal);
        Assert.Contains("row.MetricCombo.SelectedIndex = -1;", source, StringComparison.Ordinal);
        Assert.Contains("row.SubtypeCombo.SelectedIndex = -1;", source, StringComparison.Ordinal);
        Assert.Contains("await LoadMetricOptionsAsync();", source, StringComparison.Ordinal);
        Assert.Contains("EnsureInputRowCount(1);", source, StringComparison.Ordinal);
        Assert.Contains("ClearEquationErrorState();", source, StringComparison.Ordinal);
        Assert.Contains("SetSummaryStatus(\"Select inputs and an operation, then compute an Operation Chain result.\");", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidEquationErrors_ShouldUseValidationErrorBrush()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "UI",
            "OperationChain",
            "OperationChainWorkbenchView.xaml.cs");
        var lightTheme = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "UI",
            "Theming",
            "Theme.Light.xaml");
        var darkTheme = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "UI",
            "Theming",
            "Theme.Dark.xaml");

        Assert.Contains("ThemeValidationErrorBrush", lightTheme, StringComparison.Ordinal);
        Assert.Contains("ThemeValidationErrorBrush", darkTheme, StringComparison.Ordinal);
        Assert.Contains("TransformEquationValidationException", source, StringComparison.Ordinal);
        Assert.Contains("SetSummaryError($\"Invalid equation: {ex.Message}\");", source, StringComparison.Ordinal);
        Assert.Contains("SummaryText.SetResourceReference(TextBlock.ForegroundProperty, SummaryErrorBrushResourceKey);", source, StringComparison.Ordinal);
        Assert.Contains("SummaryText.SetResourceReference(TextBlock.ForegroundProperty, SummaryStatusBrushResourceKey);", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ControlOnlyChanges_ShouldNotRefreshSelectedDateRange()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "UI",
            "OperationChain",
            "OperationChainWorkbenchView.xaml.cs");

        var operationChangedIndex = source.IndexOf("private void OnOperationChanged", StringComparison.Ordinal);
        var resolutionChangedIndex = source.IndexOf("private async void OnResolutionChanged", StringComparison.Ordinal);
        Assert.True(operationChangedIndex >= 0);
        Assert.True(resolutionChangedIndex > operationChangedIndex);

        var operationChangedBody = source.Substring(operationChangedIndex, resolutionChangedIndex - operationChangedIndex);
        Assert.DoesNotContain("RefreshDateRangeForSelectedInputsAsync", operationChangedBody, StringComparison.Ordinal);

        Assert.Contains("subtypeCombo.SelectionChanged += (_, _) =>", source, StringComparison.Ordinal);
        Assert.DoesNotContain("subtypeCombo.SelectionChanged += async", source, StringComparison.Ordinal);
    }
}
