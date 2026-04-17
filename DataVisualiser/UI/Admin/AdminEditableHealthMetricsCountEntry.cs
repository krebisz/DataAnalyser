using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Admin;

internal sealed class AdminEditableHealthMetricsCountEntry : INotifyPropertyChanged
{
    private readonly HealthMetricsCountEntry _original;
    private bool _disabled;
    private bool _isDirty;
    private string _metricSubtypeName;
    private string _metricTypeName;

    public AdminEditableHealthMetricsCountEntry(HealthMetricsCountEntry source)
    {
        _original = source ?? throw new ArgumentNullException(nameof(source));
        MetricType = source.MetricType;
        MetricSubtype = source.MetricSubtype;
        RecordCount = source.RecordCount;
        MostRecentDateTime = source.MostRecentDateTime;

        _metricTypeName = source.MetricTypeName ?? string.Empty;
        _metricSubtypeName = source.MetricSubtypeName ?? string.Empty;
        _disabled = source.Disabled;
        _isDirty = false;
    }

    public string MetricType { get; }
    public string MetricSubtype { get; }
    public string IdentityKey => $"{MetricType}:{MetricSubtype}";
    public long RecordCount { get; }
    public DateTime? MostRecentDateTime { get; }

    public string MetricTypeName
    {
        get => _metricTypeName;
        set
        {
            if (value == _metricTypeName)
                return;
            _metricTypeName = value ?? string.Empty;
            OnPropertyChanged();
            RecomputeDirty();
        }
    }

    public string MetricSubtypeName
    {
        get => _metricSubtypeName;
        set
        {
            if (value == _metricSubtypeName)
                return;
            _metricSubtypeName = value ?? string.Empty;
            OnPropertyChanged();
            RecomputeDirty();
        }
    }

    public bool Disabled
    {
        get => _disabled;
        set
        {
            if (value == _disabled)
                return;
            _disabled = value;
            OnPropertyChanged();
            RecomputeDirty();
        }
    }

    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (value == _isDirty)
                return;
            _isDirty = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public HealthMetricsCountEntry ToUpdate()
    {
        return new HealthMetricsCountEntry
        {
            MetricType = MetricType,
            MetricSubtype = MetricSubtype,
            MetricTypeName = MetricTypeName,
            MetricSubtypeName = MetricSubtypeName,
            Disabled = Disabled
        };
    }

    public void AcceptChanges()
    {
        _original.MetricTypeName = MetricTypeName;
        _original.MetricSubtypeName = MetricSubtypeName;
        _original.Disabled = Disabled;
        IsDirty = false;
    }

    private void RecomputeDirty()
    {
        IsDirty = !string.Equals(_original.MetricTypeName ?? string.Empty, MetricTypeName, StringComparison.Ordinal) ||
                  !string.Equals(_original.MetricSubtypeName ?? string.Empty, MetricSubtypeName, StringComparison.Ordinal) ||
                  _original.Disabled != Disabled;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
