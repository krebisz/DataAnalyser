using DataVisualiser.Core.Data;
using DataVisualiser.Core.Data.Abstractions;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Validation.DataLoad;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Events;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.ViewModels;

public sealed class MetricLoadCoordinatorTests
{
    [Fact]
    public async Task LoadSubtypesAsync_ShouldExposeLoadingFlagAsFalseInsideLoadedCallback()
    {
        var chartState = new ChartState();
        var metricState = new MetricState
        {
            SelectedMetricType = "Weight",
            ResolutionTableName = "HealthMetrics"
        };
        var uiState = new UiState();
        var validator = new DataLoadValidator(metricState);
        var service = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        var coordinator = MetricLoadCoordinator.CreateInstance(
            chartState,
            metricState,
            uiState,
            service,
            validator,
            ex => ex.Message);

        bool? loadingFlagInsideCallback = null;
        IReadOnlyList<MetricNameOption>? loadedSubtypes = null;

        await coordinator.LoadSubtypesAsync(
            args =>
            {
                loadingFlagInsideCallback = uiState.IsLoadingSubtypes;
                loadedSubtypes = args.Subtypes.ToList();
            },
            message => throw new Xunit.Sdk.XunitException(message));

        Assert.False(loadingFlagInsideCallback);
        Assert.False(uiState.IsLoadingSubtypes);
        Assert.NotNull(loadedSubtypes);
        Assert.Equal(2, loadedSubtypes!.Count);
    }

    private sealed class StubMetricSelectionDataQueries : IMetricSelectionDataQueries
    {
        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null)
        {
            return Task.FromResult(0L);
        }

        public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(
            string baseType,
            string? subtype,
            DateTime? from,
            DateTime? to,
            string tableName,
            int? maxRecords = null,
            SamplingMode samplingMode = SamplingMode.None,
            int? targetSamples = null)
        {
            return Task.FromResult<IEnumerable<MetricData>>(Array.Empty<MetricData>());
        }

        public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName)
        {
            return Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        }

        public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName)
        {
            IEnumerable<MetricNameOption> results =
            [
                new MetricNameOption("morning", "Morning"),
                new MetricNameOption("evening", "Evening")
            ];

            return Task.FromResult(results);
        }

        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype, string tableName)
        {
            return Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        }

        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null)
        {
            return Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        }

        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName)
        {
            return Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        }
    }
}
