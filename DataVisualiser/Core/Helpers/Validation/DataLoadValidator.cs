using DataVisualiser.UI.State;

namespace DataVisualiser.Core.Helpers.Validation;

public sealed class DataLoadValidator
{
    private readonly MetricState _metricState;

    public DataLoadValidator(MetricState metricState)
    {
        _metricState = metricState ?? throw new ArgumentNullException(nameof(metricState));
    }

    public bool ValidateMetricTypeSelected()
    {
        return !string.IsNullOrWhiteSpace(_metricState.SelectedMetricType);
    }

    public bool ValidateMetricTypeSelected(out string message)
    {
        if (!ValidateMetricTypeSelected())
        {
            message = "Please select a Metric Type before loading data.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    public bool ValidateDateRange()
    {
        if (!_metricState.FromDate.HasValue || !_metricState.ToDate.HasValue)
            return false;

        return _metricState.FromDate.Value <= _metricState.ToDate.Value;
    }

    public bool ValidateDateRange(out string message)
    {
        if (!_metricState.FromDate.HasValue || !_metricState.ToDate.HasValue)
        {
            message = "Please select both From and To dates before loading data.";
            return false;
        }

        if (_metricState.FromDate > _metricState.ToDate)
        {
            message = "From date must be before To date.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    public(bool IsValid, string? ErrorMessage) ValidateDataLoadRequirements()
    {
        if (!ValidateDataLoadRequirements(out var message))
            return (false, message);

        return (true, null);
    }

    public bool ValidateDataLoadRequirements(out string message)
    {
        if (!ValidateMetricTypeSelected(out message))
            return false;

        if (!ValidateDateRange(out message))
            return false;

        message = string.Empty;
        return true;
    }
}