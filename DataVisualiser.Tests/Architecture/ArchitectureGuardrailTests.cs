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
    public void EvidenceDiagnosticsBuilder_ShouldNotDependBackOnExportServiceHelpers()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainHost", "EvidenceDiagnosticsBuilder.cs");

        Assert.DoesNotContain("MainChartsEvidenceExportService.IsSameSelection", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsEvidenceExportService_ShouldDelegateParityAssemblyToDedicatedBuilder()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainHost", "MainChartsEvidenceExportService.cs");

        Assert.Contains("EvidenceParityBuilder", source, StringComparison.Ordinal);
        Assert.Contains("_parityBuilder.BuildAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("BuildTransformParitySnapshotAsync(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("BuildMultiMetricParitySnapshotAsync(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ExecuteParitySafe(", source, StringComparison.Ordinal);
    }

    [Fact]
    public void EvidenceParityBuilder_ShouldDelegateTransformParityToDedicatedEvaluator()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainHost", "EvidenceParityBuilder.cs");

        Assert.Contains("EvidenceTransformParityEvaluator", source, StringComparison.Ordinal);
        Assert.Contains("_transformParityEvaluator.BuildAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ComputeBinaryTransformParity(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ComputeUnaryTransformParity(", source, StringComparison.Ordinal);
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
    public void MainChartsView_OnAnySubtypeSelectionChanged_ShouldNotClearLoadedChartsJustBecauseSelectionChanged()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var methodBody = ExtractMethodBody(source, "private async void OnAnySubtypeSelectionChanged");

        Assert.Contains("UpdateSelectedSubtypesInViewModel();", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("ClearAllCharts();", methodBody, StringComparison.Ordinal);
        Assert.Contains("if (HasLoadedData())", methodBody, StringComparison.Ordinal);
        Assert.Contains("await RenderChartsFromLastContext();", methodBody, StringComparison.Ordinal);
        Assert.Contains("await LoadDateRangeForSelectedMetrics();", methodBody, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ToggleEnablement_ShouldUseLoadedContextCapabilities_NotSelectionCompatibility()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var primaryMethodBody = ExtractMethodBody(source, "private void UpdatePrimaryDataRequiredButtonStates");
        var secondaryMethodBody = ExtractMethodBody(source, "private void UpdateSecondaryDataRequiredButtonStates");

        Assert.Contains("MainChartsViewToggleStateEvaluator.CanTogglePrimaryCharts(_viewModel.ChartState.LastContext)", primaryMethodBody, StringComparison.Ordinal);
        Assert.Contains("MainChartsViewToggleStateEvaluator.HasLoadedSecondaryData(_viewModel.ChartState.LastContext)", secondaryMethodBody, StringComparison.Ordinal);
        Assert.Contains("MainChartsViewToggleStateEvaluator.CanToggleSecondaryCharts(_viewModel.ChartState.LastContext)", secondaryMethodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("HasLoadedData()", primaryMethodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("HasLoadedData()", secondaryMethodBody, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartAdapters_ShouldUseSharedTimeBucketAggregationHelper()
    {
        var barPieSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "BarPieChartControllerAdapter.cs");
        var syncfusionSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "SyncfusionSunburstChartControllerAdapter.cs");

        Assert.Contains("TimeBucketAggregationHelper.BuildAverageTotals", barPieSource, StringComparison.Ordinal);
        Assert.Contains("TimeBucketAggregationHelper.BuildAverageTotals", syncfusionSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private static double?[] BuildBucketTotals", barPieSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private static double?[] BuildBucketTotals", syncfusionSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private static int ResolveBucketIndex", barPieSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private static int ResolveBucketIndex", syncfusionSource, StringComparison.Ordinal);
    }

    [Fact]
    public void TransformExecutionCoordinator_ShouldDelegateTransformComputationToSharedService()
    {
        var adapterSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "TransformDataPanelControllerAdapter.cs");
        var coordinatorSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "TransformOperationExecutionCoordinator.cs");

        Assert.Contains("_transformOperationExecutionCoordinator.Execute", adapterSource, StringComparison.Ordinal);
        Assert.Contains("_transformComputationService.ComputeUnaryTransform", coordinatorSource, StringComparison.Ordinal);
        Assert.Contains("_transformComputationService.ComputeBinaryTransform", coordinatorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("PrepareMetricData", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ComputeUnaryTransform(", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ComputeBinaryTransform(", adapterSource, StringComparison.Ordinal);
    }

    [Fact]
    public void TransformAdapter_ShouldDelegateSelectionResolutionAndExecutionToDedicatedCoordinators()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "TransformDataPanelControllerAdapter.cs");

        Assert.Contains("TransformDataResolutionCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("TransformOperationExecutionCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("TransformOperationStateCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("TransformSessionMilestoneRecorder", source, StringComparison.Ordinal);
        Assert.Contains("_transformDataResolutionCoordinator.ResolveSelections", source, StringComparison.Ordinal);
        Assert.Contains("_transformDataResolutionCoordinator.ResolveAsync", source, StringComparison.Ordinal);
        Assert.Contains("_transformOperationExecutionCoordinator.Execute", source, StringComparison.Ordinal);
        Assert.Contains("_transformOperationStateCoordinator.UpdateComputeButtonState", source, StringComparison.Ordinal);
        Assert.Contains("_transformOperationStateCoordinator.GetSelectedOperationTag", source, StringComparison.Ordinal);
        Assert.Contains("_transformSessionMilestoneRecorder.RecordExecution", source, StringComparison.Ordinal);
        Assert.Contains("_transformSessionMilestoneRecorder.RecordToggle", source, StringComparison.Ordinal);
        Assert.Contains("TransformSubtypeSelectionCoordinator.ApplySubtypeOptions", source, StringComparison.Ordinal);
        Assert.Contains("TransformSubtypeSelectionCoordinator.ResetSelectionControls", source, StringComparison.Ordinal);
        Assert.Contains("TransformGridPresentationCoordinator.PopulateInputGrids", source, StringComparison.Ordinal);
        Assert.Contains("TransformGridPresentationCoordinator.PopulateResultGrid", source, StringComparison.Ordinal);
        Assert.Contains("TransformChartPresentationCoordinator.RenderResultsAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private bool CanUpdateTransformSubtypeOptions()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void ResetTransformSelectionControls()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void UpdatePrimaryTransformSubtype(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void UpdateSecondaryTransformSubtype(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void PopulateTransformResultGrid(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private async Task RenderTransformChart(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private async Task<IReadOnlyList<MetricData>?> ResolveTransformDataAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private MetricSeriesSelection? ResolveSelectedTransformPrimarySeries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private MetricSeriesSelection? ResolveSelectedTransformSecondarySeries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static ChartDataContext BuildTransformContext", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void RecordTransformMilestone", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void RecordTransformToggleMilestone", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private bool TryGetSelectedOperation", source, StringComparison.Ordinal);
    }

    [Fact]
    public void TransformOperationStateCoordinator_ShouldOwnComputeButtonExecutionEligibility()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "TransformOperationStateCoordinator.cs");

        Assert.Contains("TransformDataResolutionCoordinator.CanRenderPrimarySelection", source, StringComparison.Ordinal);
        Assert.Contains("executionCoordinator.CanExecute", source, StringComparison.Ordinal);
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
    public void DataFetcher_ShouldRemainFacadeOverFocusedQueryGroups()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Data", "Repositories", "DataFetcher.cs");

        Assert.Contains("DataFetcherMetricCatalogQueries", source, StringComparison.Ordinal);
        Assert.Contains("DataFetcherMetricDataQueries", source, StringComparison.Ordinal);
        Assert.Contains("DataFetcherDateRangeQueries", source, StringComparison.Ordinal);
        Assert.Contains("DataFetcherAdminQueries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SELECT ", source, StringComparison.Ordinal);
        Assert.DoesNotContain("MERGE ", source, StringComparison.Ordinal);
        Assert.DoesNotContain("QueryAsync<", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ExecuteAsync(", source, StringComparison.Ordinal);
    }

    [Fact]
    public void BaseDistributionService_ShouldDelegatePureDistributionComputationToSharedHelper()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Services", "BaseDistributionService.cs");

        Assert.Contains("DistributionComputationHelper.GetBucketValues", source, StringComparison.Ordinal);
        Assert.Contains("DistributionComputationHelper.CalculateGlobalMinMax", source, StringComparison.Ordinal);
        Assert.Contains("DistributionComputationHelper.CalculateTooltipData", source, StringComparison.Ordinal);
        Assert.Contains("DistributionComputationHelper.CalculateSimpleRangeTooltipData", source, StringComparison.Ordinal);
        Assert.Contains("DistributionComputationHelper.CalculateBucketAverages", source, StringComparison.Ordinal);
        Assert.Contains("DistributionRangeResultBuilder.Build", source, StringComparison.Ordinal);
        Assert.Contains("DistributionSeriesBuilder.AddBaselineAndRangeSeries", source, StringComparison.Ordinal);
        Assert.Contains("DistributionSeriesBuilder.AddAverageSeries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("protected void AddBaselineAndRangeSeries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("protected void AddAverageSeries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("protected StackedColumnSeries CreateBaselineSeries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("protected StackedColumnSeries CreateRangeSeries", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartHelper_ShouldDelegateTooltipFormattingToDedicatedHelper()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Rendering", "Helpers", "ChartHelper.cs");

        Assert.Contains("ChartTooltipFormattingHelper.GetChartValuesAtIndex", source, StringComparison.Ordinal);
        Assert.Contains("ChartTooltipFormattingHelper.GetChartValuesFormattedAtIndex", source, StringComparison.Ordinal);
        Assert.Contains("ChartTooltipFormattingHelper.ParseSeriesTitle", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static string BuildStackedValuesFormattedString", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static string BuildCumulativeTooltipFromSeries", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartRenderEngine_ShouldDelegateSeriesFormattingAndMaterializationToDedicatedHelpers()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Rendering", "Engines", "ChartRenderEngine.cs");

        Assert.Contains("ChartSeriesMaterializer.CreateAndPopulateSeries", source, StringComparison.Ordinal);
        Assert.Contains("ChartSeriesMaterializer.ResolveStackedSeriesValues", source, StringComparison.Ordinal);
        Assert.Contains("ChartSeriesLabelFormatter.FormatSeriesLabel", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static LineSeries CreateAndPopulateSeries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static IList<double> ResolveStackedSeriesValues", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static string FormatMetricLabel", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartUpdateCoordinator_ShouldDelegateCumulativeAndYAxisPreparationToDedicatedHelpers()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Orchestration", "ChartUpdateCoordinator.cs");

        Assert.Contains("ChartCumulativeSeriesBuilder.Build", source, StringComparison.Ordinal);
        Assert.Contains("ChartYAxisDataBuilder.BuildSyntheticRawData", source, StringComparison.Ordinal);
        Assert.Contains("ChartYAxisDataBuilder.CollectSmoothedValues", source, StringComparison.Ordinal);
        Assert.Contains("ChartYAxisDataBuilder.EnsureOverlayExtremes", source, StringComparison.Ordinal);
        Assert.Contains("ChartYAxisDataBuilder.EnsureStackedBaseline", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private(List<SeriesResult>? RenderSeries, List<SeriesResult>? OriginalSeries) BuildCumulativeSeriesFromMulti", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private List<double> BuildStackedSmoothedValues", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static void EnsureOverlayExtremes", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldDelegateSessionDiagnosticsBookkeepingToDedicatedRecorder()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("MainChartsSessionDiagnosticsRecorder", source, StringComparison.Ordinal);
        Assert.Contains("_sessionDiagnosticsRecorder.TrackHostMessage", source, StringComparison.Ordinal);
        Assert.Contains("_sessionDiagnosticsRecorder.RecordSessionMilestone", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void TrackHostMessage", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void RecordSessionMilestone", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldDelegateUiSurfaceDiagnosticsReadingToDedicatedReader()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var methodBody = ExtractMethodBody(source, "private UiSurfaceDiagnosticsSnapshot CaptureEvidenceExportUiSurfaceDiagnostics");

        Assert.Contains("MainChartsUiSurfaceDiagnosticsReader", source, StringComparison.Ordinal);
        Assert.Contains("_uiSurfaceDiagnosticsReader.Capture", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("TransformUiDiagnosticsSnapshot", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("SubtypeComboDiagnosticsSnapshot", methodBody, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldDelegateZoomResetThroughDedicatedCoordinator()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var methodBody = ExtractMethodBody(source, "private void ResetRegisteredChartsZoom");

        Assert.Contains("MainChartsViewZoomResetCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("_zoomResetCoordinator.ResetRegisteredCharts", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("controller.ResetZoom();", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("ResolveController(key).ResetZoom();", methodBody, StringComparison.Ordinal);
    }

    [Fact]
    public void VNextBridge_ShouldAcceptExplicitProgramRequests()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "MainHost", "VNextMainChartIntegrationCoordinator.cs");

        Assert.Contains("LoadProgramAsync(", source, StringComparison.Ordinal);
        Assert.Contains("ChartProgramRequest", source, StringComparison.Ordinal);
        Assert.Contains("WorkflowPlanRequest", source, StringComparison.Ordinal);
        Assert.Contains("coordinator.BuildProgram(programRequest)", source, StringComparison.Ordinal);
    }

    [Fact]
    public void VNextWorkflowState_ShouldUseExplicitWorkflowPlanRequest()
    {
        var stateSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "VNext", "State", "WorkflowState.cs");
        var transitionSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "VNext", "State", "ReasoningSessionTransitions.cs");

        Assert.Contains("WorkflowPlanRequest", stateSource, StringComparison.Ordinal);
        Assert.Contains("WorkflowPlanRequest", transitionSource, StringComparison.Ordinal);
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
            Path.Combine("DataVisualiser", "Core", "Orchestration", "ChartUpdateCoordinator.cs"),
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
        var source = SourceTreeTestHelper.ReadRepositoryFile("documents", "DataVisualiser_Subsystem_Plan.md");

        Assert.Contains("2-3", source, StringComparison.Ordinal);
        Assert.Contains("do not generalize before", source, StringComparison.OrdinalIgnoreCase);
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
