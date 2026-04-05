using System.IO;
using System.Text;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Architecture;

public sealed class ArchitectureGuardrailTests
{
    [Fact]
    public void MainChartsView_ShouldKeepExportOnDedicatedWriterSeam()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("MainChartsEvidenceExportService", source);
        Assert.DoesNotContain("File.WriteAllText(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Directory.CreateDirectory(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("JsonSerializer.Serialize(", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldNotReacquireParityAssemblyOrTransformParityHelpers()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.DoesNotContain("BuildParitySummary(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("BuildTransformParitySnapshotAsync(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TransformExpressionBuilder", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TransformExpressionEvaluator", source, StringComparison.Ordinal);
        Assert.DoesNotContain("UnaryOperators", source, StringComparison.Ordinal);
        Assert.DoesNotContain("BinaryOperators", source, StringComparison.Ordinal);
        Assert.DoesNotContain("JsonSerializer", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldRemainComposedThroughKnownHostSeams()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("MainChartsViewThemeCoordinator", source);
        Assert.Contains("MainChartsEvidenceExportService", source);
        Assert.Contains("MainChartsViewEvidenceExportCoordinator", source);
        Assert.Contains("MainChartsViewDataLoadedCoordinator", source);
        Assert.Contains("MainChartsViewChartUpdateCoordinator", source);
        Assert.Contains("MainChartsViewChartPresentationCoordinator", source);
        Assert.Contains("MainChartsViewChartPipelineFactory", source);
    }

    [Fact]
    public void MainChartsView_ShouldKeepThemeOnDedicatedCoordinatorSeam()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("MainChartsViewThemeCoordinator", source);
        Assert.DoesNotContain("ThemeChanged +=", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ThemeChanged -=", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_OnDataLoaded_ShouldDelegateThroughDedicatedCoordinatorSeam()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var onDataLoadedBody = ExtractMethodBody(source, "private async void OnDataLoaded");

        Assert.Contains("_dataLoadedCoordinator.HandleAsync", onDataLoadedBody, StringComparison.Ordinal);
        Assert.DoesNotContain("CompleteTransformSelectionsPendingLoad();", onDataLoadedBody, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateSubtypeOptions(ChartControllerKeys.Normalized);", onDataLoadedBody, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateTransformSubtypeOptions();", onDataLoadedBody, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateTransformComputeButtonState();", onDataLoadedBody, StringComparison.Ordinal);
        Assert.DoesNotContain("await RenderChartAsync(ChartControllerKeys.BarPie, ctx);", onDataLoadedBody, StringComparison.Ordinal);
        Assert.DoesNotContain("await RenderChartsFromLastContext();", onDataLoadedBody, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_UpdateSelectedSubtypes_ShouldRefreshTransformAlongsidePeerCharts()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var methodBody = ExtractMethodBody(source, "private void UpdateSelectedSubtypesInViewModel");

        Assert.Contains("UpdateTransformSubtypeOptions();", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain(".GroupBy(series => series.DisplayKey", methodBody, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_HasLoadedData_ShouldUseSharedSelectionCompatibilityGuard()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var methodBody = ExtractMethodBody(source, "private bool HasLoadedData");

        Assert.Contains("ChartContextSelectionGuard.IsCompatibleWithCurrentSelection", methodBody, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_OnMetricTypeSelectionChanged_ShouldClearSubtypeControlsBeforeLoadingNewSubtypes()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var methodBody = ExtractMethodBody(source, "private void OnMetricTypeSelectionChanged");

        Assert.Contains("_isApplyingSelectionSync = true;", methodBody, StringComparison.Ordinal);
        Assert.Contains("ClearAllCharts();", methodBody, StringComparison.Ordinal);
        Assert.Contains("_viewModel.SetSelectedMetricType(selectedMetricType);", methodBody, StringComparison.Ordinal);
        Assert.Contains("_selectorManager.ClearAllSubtypeControls();", methodBody, StringComparison.Ordinal);
        Assert.Contains("UpdateSelectedSubtypesInViewModel();", methodBody, StringComparison.Ordinal);

        Assert.True(
            methodBody.IndexOf("ClearAllCharts();", StringComparison.Ordinal) <
            methodBody.IndexOf("_viewModel.SetSelectedMetricType(selectedMetricType);", StringComparison.Ordinal),
            "Metric-type changes should clear the loaded chart context before new subtype state is committed.");

        Assert.True(
            methodBody.IndexOf("_viewModel.SetSelectedMetricType(selectedMetricType);", StringComparison.Ordinal) <
            methodBody.IndexOf("_selectorManager.ClearAllSubtypeControls();", StringComparison.Ordinal),
            "Metric type should be committed before subtype controls are cleared so selection sync cannot snap the combo back.");
    }

    [Fact]
    public void MainChartsView_OnSubtypesLoaded_ShouldNotPreserveOnlyTheLastDynamicComboOnMetricTypeChange()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var methodBody = ExtractMethodBody(source, "private void OnSubtypesLoaded");

        Assert.DoesNotContain("UpdateLastDynamicComboItems", methodBody, StringComparison.Ordinal);
        Assert.Contains("_selectorManager.SuppressSelectionChanged()", methodBody, StringComparison.Ordinal);
        Assert.Contains("_viewModel.BeginSelectionStateBatch()", methodBody, StringComparison.Ordinal);
    }

    [Fact]
    public void Hosts_ShouldBatchProgrammaticMetricAndSubtypeSelectionMutations()
    {
        var mainSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        Assert.Contains("_viewModel.BeginSelectionStateBatch()", ExtractMethodBody(mainSource, "private void OnMetricTypesLoaded"), StringComparison.Ordinal);
        Assert.Contains("_viewModel.BeginSelectionStateBatch()", ExtractMethodBody(mainSource, "private Task<bool> LoadDataAndValidate"), StringComparison.Ordinal);
        Assert.Contains("_selectorManager.SuppressSelectionChanged()", ExtractMethodBody(mainSource, "private void AddSubtypeComboBox"), StringComparison.Ordinal);

        var syncfusionSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Syncfusion", "SyncfusionChartsView.xaml.cs");
        Assert.Contains("_viewModel.BeginSelectionStateBatch()", ExtractMethodBody(syncfusionSource, "private void OnMetricTypesLoaded"), StringComparison.Ordinal);
        Assert.Contains("_viewModel.BeginSelectionStateBatch()", ExtractMethodBody(syncfusionSource, "private Task<bool> LoadDataAndValidate"), StringComparison.Ordinal);
        Assert.Contains("_selectorManager.SuppressSelectionChanged()", ExtractMethodBody(syncfusionSource, "private void AddSubtypeComboBox"), StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsViewAndMainHost_ShouldNotReintroduceNamedControlFallbacks()
    {
        var mainChartsViewSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        Assert.DoesNotContain("FindName(", mainChartsViewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("VisualTreeHelper", mainChartsViewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LogicalTreeHelper", mainChartsViewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("GetType().GetField", mainChartsViewSource, StringComparison.Ordinal);

        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [Path.Combine("DataVisualiser", "UI", "MainHost")],
            ["FindName(", "VisualTreeHelper", "LogicalTreeHelper", "GetType().GetField"]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void CoreValidation_DataLoadHelpers_ShouldNotReferenceUiEventsNamespace()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Validation", "DataLoad", "MetricDataValidationHelper.cs");

        Assert.DoesNotContain("DataVisualiser.UI.Events", source, StringComparison.Ordinal);
    }

    [Fact]
    public void CoreOrchestrationAndMainHost_ShouldNotReferenceSyncfusionOrLiveChartsCore()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [
                Path.Combine("DataVisualiser", "Core", "Orchestration"),
                Path.Combine("DataVisualiser", "UI", "MainHost")
            ],
            ["Syncfusion", "LiveChartsCore"]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void CoreOrchestration_ShouldNotOwnThemeOrExportLogic()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [Path.Combine("DataVisualiser", "Core", "Orchestration")],
            ["AppThemeService", "ThemeChanged", "ReachabilityExportWriter", "JsonSerializer", "File.WriteAllText(", "Directory.CreateDirectory(", "reachability-", "documents\\"]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void GuardedRuntimeFiles_ShouldNotUseSilentMessageBoxSingletonFallbacks()
    {
        var guardedFiles = new[]
        {
            Path.Combine("DataVisualiser", "Core", "Orchestration", "Coordinator", "ChartUpdateCoordinator.cs"),
            Path.Combine("DataVisualiser", "Core", "Orchestration", "SecondaryCharts", "SecondaryMetricChartOrchestrationPipeline.cs"),
            Path.Combine("DataVisualiser", "Core", "Orchestration", "DistributionCharts", "DistributionChartOrchestrationPipeline.cs"),
            Path.Combine("DataVisualiser", "Core", "Services", "BaseDistributionService.cs")
        };

        foreach (var relativeFile in guardedFiles)
        {
            var source = SourceTreeTestHelper.ReadRepositoryFile(relativeFile);
            Assert.DoesNotContain("MessageBoxUserNotificationService.Instance", source, StringComparison.Ordinal);
        }

        var compositionSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainHost", "MainChartsViewChartPipelineFactory.cs");
        Assert.Contains("MessageBoxUserNotificationService.Instance", compositionSource);
    }

    [Fact]
    public void ExecutionPlan_ShouldKeepLateGeneralizationGuardrailsDocumented()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("documents", "ARCHITECTURE_REHAUL_CONSOLIDATED_EXECUTION_PLAN.md");

        Assert.Contains("2-3", source, StringComparison.Ordinal);
        Assert.Contains("logical component/layer", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("thin adapters, pure renderers, and simple state holders", source, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertNoMatches(IReadOnlyList<string> offenders)
    {
        if (offenders.Count == 0)
            return;

        var builder = new StringBuilder();
        builder.AppendLine("Forbidden architectural references were found:");
        foreach (var offender in offenders)
            builder.AppendLine(offender);

        Assert.True(offenders.Count == 0, builder.ToString());
    }

    private static string ExtractMethodBody(string source, string signature)
    {
        var start = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Method signature '{signature}' was not found.");

        var braceStart = source.IndexOf('{', start);
        Assert.True(braceStart >= 0, $"Opening brace for '{signature}' was not found.");

        var depth = 0;
        for (var i = braceStart; i < source.Length; i++)
        {
            if (source[i] == '{')
                depth++;
            else if (source[i] == '}')
                depth--;

            if (depth == 0)
                return source.Substring(braceStart, i - braceStart + 1);
        }

        throw new InvalidOperationException($"Closing brace for '{signature}' was not found.");
    }
}
