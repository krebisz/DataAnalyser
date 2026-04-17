using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DataVisualiser.UI.Admin;

internal sealed class AdminMetricsManagerCoordinator
{
    public const string AllMetricTypesToken = "(All)";

    private readonly HashSet<string> _loggedDirtyRows = new(StringComparer.Ordinal);
    private readonly AdminSessionMilestoneRecorder _milestoneRecorder;
    private readonly IAdminMetricsRepository _repository;

    public AdminMetricsManagerCoordinator(
        IAdminMetricsRepository repository,
        AdminSessionMilestoneRecorder? milestoneRecorder = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _milestoneRecorder = milestoneRecorder ?? new AdminSessionMilestoneRecorder();
    }

    public ObservableCollection<AdminEditableHealthMetricsCountEntry> Rows { get; } = new();
    public bool HideDisabled { get; private set; }
    public bool IsLoading { get; private set; }
    public bool CanSave => Rows.Any(row => row.IsDirty) && !IsLoading;

    public event EventHandler<AdminRowsChangedEventArgs>? RowsChanged;

    public async Task<IReadOnlyList<string>> LoadMetricTypesAsync()
    {
        IsLoading = true;
        try
        {
            var metricTypes = await _repository.GetMetricTypesAsync();
            return new[] { AllMetricTypesToken }.Concat(metricTypes).ToList();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<AdminCountsLoadResult> ReloadCountsAsync(string? metricType)
    {
        IsLoading = true;
        try
        {
            var rows = await _repository.GetCountsAsync(metricType);
            ReplaceRows(rows.Select(row => new AdminEditableHealthMetricsCountEntry(row)));
            _milestoneRecorder.RecordReloadCompleted(metricType, Rows.Count);
            return new AdminCountsLoadResult(true, $"Loaded {Rows.Count} row(s).", Rows.Count);
        }
        catch (Exception ex)
        {
            _milestoneRecorder.RecordReloadFailed(metricType, ex.Message);
            return new AdminCountsLoadResult(false, $"Load failed: {ex.Message}", Rows.Count);
        }
        finally
        {
            IsLoading = false;
            RaiseRowsChanged(disabledChanged: false);
        }
    }

    public async Task<AdminSaveResult> SaveAsync()
    {
        var dirty = Rows.Where(row => row.IsDirty).ToList();
        _milestoneRecorder.RecordSaveRequested(dirty.Count);
        if (dirty.Count == 0)
        {
            _milestoneRecorder.RecordSaveSkipped();
            return new AdminSaveResult(true, "No changes to save.", 0, 0);
        }

        IsLoading = true;
        try
        {
            var updates = dirty.Select(row => row.ToUpdate()).ToList();
            var affected = await _repository.UpdateCountsAsync(updates);

            foreach (var row in dirty)
            {
                row.AcceptChanges();
                _loggedDirtyRows.Remove(row.IdentityKey);
            }

            _milestoneRecorder.RecordSaveCompleted(dirty.Count, affected);
            return new AdminSaveResult(true, $"Saved. Rows updated: {affected}.", dirty.Count, affected);
        }
        catch (Exception ex)
        {
            _milestoneRecorder.RecordSaveFailed(dirty.Count, ex.Message);
            return new AdminSaveResult(false, $"Save failed: {ex.Message}", dirty.Count, 0);
        }
        finally
        {
            IsLoading = false;
            RaiseRowsChanged(disabledChanged: false);
        }
    }

    public void RecordReloadRequested(string? metricType)
    {
        _milestoneRecorder.RecordReloadRequested(metricType);
    }

    public void RecordMetricTypeChanged(string? metricType)
    {
        _milestoneRecorder.RecordMetricTypeChanged(metricType);
    }

    public void SetHideDisabled(bool hideDisabled)
    {
        HideDisabled = hideDisabled;
        _milestoneRecorder.RecordHideDisabledToggled(hideDisabled);
        RaiseRowsChanged(disabledChanged: true);
    }

    public bool ShouldIncludeRow(object item)
    {
        return !HideDisabled || item is AdminEditableHealthMetricsCountEntry { Disabled: false };
    }

    private void ReplaceRows(IEnumerable<AdminEditableHealthMetricsCountEntry> rows)
    {
        foreach (var existing in Rows)
            existing.PropertyChanged -= OnRowPropertyChanged;

        Rows.Clear();
        _loggedDirtyRows.Clear();
        foreach (var row in rows)
        {
            row.PropertyChanged += OnRowPropertyChanged;
            Rows.Add(row);
        }
    }

    private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not (nameof(AdminEditableHealthMetricsCountEntry.IsDirty) or
            nameof(AdminEditableHealthMetricsCountEntry.MetricTypeName) or
            nameof(AdminEditableHealthMetricsCountEntry.MetricSubtypeName) or
            nameof(AdminEditableHealthMetricsCountEntry.Disabled)))
        {
            return;
        }

        if (sender is AdminEditableHealthMetricsCountEntry row && row.IsDirty && _loggedDirtyRows.Add(row.IdentityKey))
            _milestoneRecorder.RecordGridEdited(row.MetricType, row.MetricSubtype, e.PropertyName);

        RaiseRowsChanged(e.PropertyName == nameof(AdminEditableHealthMetricsCountEntry.Disabled));
    }

    private void RaiseRowsChanged(bool disabledChanged)
    {
        RowsChanged?.Invoke(this, new AdminRowsChangedEventArgs(CanSave, disabledChanged));
    }
}

internal sealed record AdminRowsChangedEventArgs(bool CanSave, bool DisabledChanged);

internal sealed record AdminCountsLoadResult(bool Success, string StatusText, int RowCount);

internal sealed record AdminSaveResult(bool Success, string StatusText, int DirtyCount, int AffectedRows);
