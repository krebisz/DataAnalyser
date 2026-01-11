using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DataVisualiser.UI.State;

public class UiState : INotifyPropertyChanged
{
    private int _dynamicSubtypeCount;
    private bool _isLoadingData;
    private bool _isLoadingMetricTypes;
    private bool _isLoadingSubtypes;
    private bool _isUiBusy;

    // Prevent re-entry during loads
    public bool IsLoadingMetricTypes
    {
        get => _isLoadingMetricTypes;
        set => SetField(ref _isLoadingMetricTypes, value);
    }

    public bool IsLoadingSubtypes
    {
        get => _isLoadingSubtypes;
        set => SetField(ref _isLoadingSubtypes, value);
    }

    public bool IsLoadingData
    {
        get => _isLoadingData;
        set => SetField(ref _isLoadingData, value);
    }

    // Whether the UI should block inputs (lockout during load)
    public bool IsUiBusy
    {
        get => _isUiBusy;
        set => SetField(ref _isUiBusy, value);
    }

    // Tracking dynamic subtype controls count
    public int DynamicSubtypeCount
    {
        get => _dynamicSubtypeCount;
        set => SetField(ref _dynamicSubtypeCount, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        OnPropertyChanged(propertyName);
    }
}