using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Admin;

namespace DataVisualiser.Tests.UI.Admin;

public sealed class AdminMetricsManagerCoordinatorTests
{
    [Fact]
    public async Task LoadMetricTypesAsync_ShouldIncludeAllToken()
    {
        var repository = new FakeAdminMetricsRepository
        {
            MetricTypes = ["Weight", "Sleep"]
        };
        var coordinator = new AdminMetricsManagerCoordinator(repository);

        var metricTypes = await coordinator.LoadMetricTypesAsync();

        Assert.Equal([AdminMetricsManagerCoordinator.AllMetricTypesToken, "Weight", "Sleep"], metricTypes);
        Assert.False(coordinator.IsLoading);
    }

    [Fact]
    public async Task ReloadCountsAsync_ShouldPopulateRowsAndResetSaveState()
    {
        var repository = new FakeAdminMetricsRepository
        {
            Rows =
            [
                CreateEntry("Weight", "body_weight"),
                CreateEntry("Weight", "body_fat_mass", disabled: true)
            ]
        };
        var coordinator = new AdminMetricsManagerCoordinator(repository);

        var result = await coordinator.ReloadCountsAsync("Weight");

        Assert.True(result.Success);
        Assert.Equal(2, result.RowCount);
        Assert.Equal(2, coordinator.Rows.Count);
        Assert.False(coordinator.CanSave);
        Assert.False(coordinator.IsLoading);
    }

    [Fact]
    public async Task RowEdit_ShouldMarkSaveAvailableAndSaveShouldPersistDirtyRows()
    {
        var repository = new FakeAdminMetricsRepository
        {
            Rows = [CreateEntry("Weight", "body_weight")]
        };
        var coordinator = new AdminMetricsManagerCoordinator(repository);
        await coordinator.ReloadCountsAsync("Weight");
        var rowChanges = new List<AdminRowsChangedEventArgs>();
        coordinator.RowsChanged += (_, args) => rowChanges.Add(args);

        coordinator.Rows[0].MetricSubtypeName = "Body Weight";

        Assert.True(coordinator.CanSave);
        Assert.Contains(rowChanges, change => change.CanSave);

        var result = await coordinator.SaveAsync();

        Assert.True(result.Success);
        Assert.Equal(1, result.DirtyCount);
        Assert.Equal(1, result.AffectedRows);
        Assert.Single(repository.Updates);
        Assert.Equal("Body Weight", repository.Updates[0].MetricSubtypeName);
        Assert.False(coordinator.CanSave);
        Assert.False(coordinator.Rows[0].IsDirty);
    }

    [Fact]
    public async Task SaveAsync_ShouldSkipRepositoryWhenNoRowsAreDirty()
    {
        var repository = new FakeAdminMetricsRepository
        {
            Rows = [CreateEntry("Weight", "body_weight")]
        };
        var coordinator = new AdminMetricsManagerCoordinator(repository);
        await coordinator.ReloadCountsAsync("Weight");

        var result = await coordinator.SaveAsync();

        Assert.True(result.Success);
        Assert.Equal(0, result.DirtyCount);
        Assert.Empty(repository.Updates);
    }

    [Fact]
    public async Task SetHideDisabled_ShouldFilterDisabledRowsAndRaiseDisabledChange()
    {
        var repository = new FakeAdminMetricsRepository
        {
            Rows =
            [
                CreateEntry("Weight", "body_weight"),
                CreateEntry("Weight", "body_fat_mass", disabled: true)
            ]
        };
        var coordinator = new AdminMetricsManagerCoordinator(repository);
        await coordinator.ReloadCountsAsync("Weight");
        AdminRowsChangedEventArgs? lastChange = null;
        coordinator.RowsChanged += (_, args) => lastChange = args;

        coordinator.SetHideDisabled(true);

        Assert.True(coordinator.HideDisabled);
        Assert.NotNull(lastChange);
        Assert.True(lastChange.DisabledChanged);
        Assert.True(coordinator.ShouldIncludeRow(coordinator.Rows[0]));
        Assert.False(coordinator.ShouldIncludeRow(coordinator.Rows[1]));
    }

    [Fact]
    public async Task DisabledEdit_ShouldRaiseDisabledChangedAndEnableSave()
    {
        var repository = new FakeAdminMetricsRepository
        {
            Rows = [CreateEntry("Weight", "body_weight")]
        };
        var coordinator = new AdminMetricsManagerCoordinator(repository);
        await coordinator.ReloadCountsAsync("Weight");
        var rowChanges = new List<AdminRowsChangedEventArgs>();
        coordinator.RowsChanged += (_, args) => rowChanges.Add(args);

        coordinator.Rows[0].Disabled = true;

        Assert.True(coordinator.CanSave);
        Assert.Contains(rowChanges, change => change.DisabledChanged);
        Assert.Contains(rowChanges, change => change.CanSave);
    }

    private static HealthMetricsCountEntry CreateEntry(string metricType, string metricSubtype, bool disabled = false)
    {
        return new HealthMetricsCountEntry
        {
            MetricType = metricType,
            MetricSubtype = metricSubtype,
            MetricTypeName = metricType,
            MetricSubtypeName = metricSubtype,
            Disabled = disabled,
            RecordCount = 10,
            MostRecentDateTime = new DateTime(2024, 1, 1)
        };
    }

    private sealed class FakeAdminMetricsRepository : IAdminMetricsRepository
    {
        public IReadOnlyList<string> MetricTypes { get; init; } = [];
        public IReadOnlyList<HealthMetricsCountEntry> Rows { get; init; } = [];
        public List<HealthMetricsCountEntry> Updates { get; } = [];

        public Task<IReadOnlyList<string>> GetMetricTypesAsync()
        {
            return Task.FromResult(MetricTypes);
        }

        public Task<IReadOnlyList<HealthMetricsCountEntry>> GetCountsAsync(string? metricType)
        {
            return Task.FromResult<IReadOnlyList<HealthMetricsCountEntry>>(Rows.Where(row => metricType == null || row.MetricType == metricType).ToList());
        }

        public Task<int> UpdateCountsAsync(IEnumerable<HealthMetricsCountEntry> updates)
        {
            Updates.AddRange(updates);
            return Task.FromResult(Updates.Count);
        }
    }
}
