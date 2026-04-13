using System.Windows.Controls;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewStateSyncCoordinatorTests
{
    [Fact]
    public void Apply_ShouldProjectStateThroughActions()
    {
        var chartState = new ChartState { BarPieBucketCount = 7 };
        var metricState = new MetricState
        {
            SelectedMetricType = "Weight",
            FromDate = new DateTime(2026, 4, 1),
            ToDate = new DateTime(2026, 4, 2),
            ResolutionTableName = DataAccessDefaults.HealthMetricsHourTable
        };
        metricState.SetSeriesSelections(
        [
            new DataVisualiser.Shared.Models.MetricSeriesSelection("Weight", "body_fat_mass"),
            new DataVisualiser.Shared.Models.MetricSeriesSelection("Weight", "fat_free_mass")
        ]);
        var viewModel = new MainWindowViewModel(chartState, metricState, new UiState(), new DataVisualiser.Core.Services.MetricSelectionService("TestConnection"));
        var coordinator = new MainChartsViewStateSyncCoordinator();

        string? resolution = null;
        DateTime? fromDate = null;
        DateTime? toDate = null;
        DataVisualiser.Shared.Models.MetricNameOption? selectedMetric = null;
        IReadOnlyList<DataVisualiser.Shared.Models.MetricSeriesSelection>? syncedSelections = null;
        DataVisualiser.Shared.Models.MetricNameOption? syncedMetricType = null;
        int bucketCount = 0;

        coordinator.Apply(
            viewModel,
            [new DataVisualiser.Shared.Models.MetricNameOption("Weight", "Weight")],
            new MainChartsViewStateSyncCoordinator.Actions(
                target => resolution = target,
                value => fromDate = value,
                value => toDate = value,
                option => selectedMetric = option,
                (selections, metricType) =>
                {
                    syncedSelections = selections;
                    syncedMetricType = metricType;
                },
                count => bucketCount = count));

        Assert.Equal("Hourly", resolution);
        Assert.Equal(metricState.FromDate, fromDate);
        Assert.Equal(metricState.ToDate, toDate);
        Assert.Equal("Weight", selectedMetric?.Value);
        Assert.NotNull(syncedSelections);
        Assert.Equal(2, syncedSelections!.Count);
        Assert.Equal("Weight", syncedMetricType?.Value);
        Assert.Equal(7, bucketCount);
    }

    [Fact]
    public void ApplyComboSelectionByValue_ShouldSelectMatchingSubtype()
    {
        StaTestHelper.Run(() =>
        {
            var combo = new ComboBox();
            combo.Items.Add(new DataVisualiser.Shared.Models.MetricNameOption("body_fat_mass", "Body Fat"));
            combo.Items.Add(new DataVisualiser.Shared.Models.MetricNameOption("fat_free_mass", "Fat Free"));

            MainChartsViewStateSyncCoordinator.ApplyComboSelectionByValue(combo, "fat_free_mass");

            var selected = Assert.IsType<DataVisualiser.Shared.Models.MetricNameOption>(combo.SelectedItem);
            Assert.Equal("fat_free_mass", selected.Value);
        });
    }
}
