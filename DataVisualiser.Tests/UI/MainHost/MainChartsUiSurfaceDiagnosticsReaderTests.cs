using System.Windows;
using System.Windows.Controls;
using DataVisualiser.Core.Data;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsUiSurfaceDiagnosticsReaderTests
{
    [Fact]
    public void Capture_ShouldProjectCurrentUiSurfaceState()
    {
        StaTestHelper.Run(() =>
        {
            var viewModel = CreateViewModel();
            var recorder = new MainChartsSessionDiagnosticsRecorder(viewModel);
            var metricCombo = new ComboBox();
            var primarySubtypeCombo = new ComboBox();
            var panel = new StackPanel();
            var metricOption = new MetricNameOption("Weight", "Weight");
            var subtypeOptions = new[]
            {
                new MetricNameOption("body_fat_mass", "Fat (mass)"),
                new MetricNameOption("fat_free_mass", "Fat Free (mass)")
            };

            metricCombo.Items.Add(metricOption);
            metricCombo.SelectedItem = metricOption;

            var selectorManager = new SubtypeSelectorManager(panel, primarySubtypeCombo);
            selectorManager.ReplacePrimaryItems(subtypeOptions, metricOption, preserveSelection: false);
            selectorManager.SetPrimaryMetricType(metricOption);
            var dynamicCombo = selectorManager.AddSubtypeCombo(subtypeOptions, metricOption);
            dynamicCombo.SelectedItem = subtypeOptions[1];

            var transformController = new FakeTransformDataPanelController();
            transformController.Visibility = Visibility.Visible;
            transformController.TransformSecondarySubtypePanel.Visibility = Visibility.Visible;
            transformController.TransformComputeButton.IsEnabled = true;
            transformController.TransformPrimarySubtypeCombo.Items.Add(subtypeOptions[0]);
            transformController.TransformPrimarySubtypeCombo.SelectedItem = subtypeOptions[0];
            transformController.TransformSecondarySubtypeCombo.Items.Add(subtypeOptions[1]);
            transformController.TransformSecondarySubtypeCombo.SelectedItem = subtypeOptions[1];
            transformController.TransformOperationCombo.Items.Add(new ComboBoxItem { Tag = "Add", Content = "Add" });
            transformController.TransformOperationCombo.SelectedIndex = 0;

            recorder.TrackHostMessage("Info", "Ready", MessageBoxImage.Information);

            var reader = new MainChartsUiSurfaceDiagnosticsReader(selectorManager, recorder);
            var snapshot = reader.Capture(
                metricCombo,
                new DatePicker { SelectedDate = DateTime.UtcNow.Date.AddDays(-30) },
                new DatePicker { SelectedDate = DateTime.UtcNow.Date },
                transformController);

            Assert.Equal("Weight", snapshot.MetricType.SelectedValue);
            Assert.Equal(2, snapshot.Subtypes.ActiveComboCount);
            Assert.True(snapshot.Subtypes.PrimarySelectionMaterialized);
            Assert.True(snapshot.Subtypes.AllCombosBoundToSelectedMetricType);
            Assert.Equal("Add", snapshot.Transform.SelectedOperation);
            Assert.Equal("body_fat_mass", snapshot.Transform.SelectedPrimarySubtype);
            Assert.Equal("fat_free_mass", snapshot.Transform.SelectedSecondarySubtype);
            Assert.True(snapshot.Transform.ComputeEnabled);
            Assert.Single(snapshot.RecentMessages);
        });
    }

    private static MainWindowViewModel CreateViewModel()
    {
        var chartState = new ChartState();
        var metricState = new MetricState();
        var uiState = new UiState();
        var metricService = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        return new MainWindowViewModel(chartState, metricState, uiState, metricService);
    }

    private sealed class FakeTransformDataPanelController : UserControl, ITransformDataPanelController
    {
        public ChartPanelController Panel { get; } = new();
        public Button ToggleButton => Panel.ToggleButtonControl;
        public CartesianChart Chart { get; } = new();
        public ComboBox TransformPrimarySubtypeCombo { get; } = new();
        public ComboBox TransformSecondarySubtypeCombo { get; } = new();
        public ComboBox TransformOperationCombo { get; } = new();
        public Button TransformComputeButton { get; } = new();
        public StackPanel TransformSecondarySubtypePanel { get; } = new();
        public StackPanel TransformGrid2Panel { get; } = new();
        public StackPanel TransformGrid3Panel { get; } = new();
        public StackPanel TransformChartContentPanel { get; } = new();
        public Grid TransformChartContainer { get; } = new();
        public DataGrid TransformGrid1 { get; } = new();
        public DataGrid TransformGrid2 { get; } = new();
        public DataGrid TransformGrid3 { get; } = new();
        public TextBlock TransformGrid1Title { get; } = new();
        public TextBlock TransformGrid2Title { get; } = new();
        public TextBlock TransformGrid3Title { get; } = new();
        public CartesianChart ChartTransformResult => Chart;
        public System.Windows.Threading.Dispatcher Dispatcher => System.Windows.Threading.Dispatcher.CurrentDispatcher;

        public event EventHandler? ToggleRequested;
        public event EventHandler? OperationChanged;
        public event EventHandler? PrimarySubtypeChanged;
        public event EventHandler? SecondarySubtypeChanged;
        public event EventHandler? ComputeRequested;
    }

    private sealed class StubMetricSelectionDataQueries : IMetricSelectionDataQueries
    {
        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null) => Task.FromResult(0L);
        public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype, DateTime? from, DateTime? to, string tableName, int? maxRecords = null, SamplingMode samplingMode = SamplingMode.None, int? targetSamples = null) => Task.FromResult<IEnumerable<MetricData>>([]);
        public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>([]);
        public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>([]);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
    }
}
