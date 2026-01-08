namespace DataVisualiser.UI.ViewModels;

public partial class MainWindowViewModel
{
    // ======================
    // VALIDATION
    // ======================

    public bool ValidateMetricTypeSelected()
    {
        return _dataLoadValidator.ValidateMetricTypeSelected();
    }

    private bool ValidateMetricTypeSelected(out string message)
    {
        return _dataLoadValidator.ValidateMetricTypeSelected(out message);
    }

    public bool ValidateDateRange()
    {
        return _dataLoadValidator.ValidateDateRange();
    }

    private bool ValidateDateRange(out string message)
    {
        return _dataLoadValidator.ValidateDateRange(out message);
    }

    public(bool IsValid, string? ErrorMessage) ValidateDataLoadRequirements()
    {
        return _dataLoadValidator.ValidateDataLoadRequirements();
    }

    private bool ValidateDataLoadRequirements(out string message)
    {
        return _dataLoadValidator.ValidateDataLoadRequirements(out message);
    }
}