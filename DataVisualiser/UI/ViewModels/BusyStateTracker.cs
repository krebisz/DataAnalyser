using System;
using System.ComponentModel;

using DataVisualiser.UI.State;

namespace DataVisualiser.UI.ViewModels;

public sealed class BusyStateTracker : INotifyPropertyChanged
{
    private readonly UiState _uiState;
    private bool _isBusy;

    public BusyStateTracker(UiState uiState)
    {
        _uiState = uiState ?? throw new ArgumentNullException(nameof(uiState));
        _isBusy = ComputeIsBusy();
        _uiState.PropertyChanged += OnUiStatePropertyChanged;
    }

    public bool IsBusy => _isBusy;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnUiStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.PropertyName) ||
            e.PropertyName == nameof(UiState.IsLoadingMetricTypes) ||
            e.PropertyName == nameof(UiState.IsLoadingSubtypes) ||
            e.PropertyName == nameof(UiState.IsLoadingData) ||
            e.PropertyName == nameof(UiState.IsUiBusy))
        {
            UpdateIsBusy();
        }
    }

    private void UpdateIsBusy()
    {
        var newValue = ComputeIsBusy();
        if (newValue == _isBusy)
            return;

        _isBusy = newValue;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
    }

    private bool ComputeIsBusy()
    {
        return _uiState.IsLoadingMetricTypes ||
            _uiState.IsLoadingSubtypes ||
            _uiState.IsLoadingData ||
            _uiState.IsUiBusy;
    }
}
