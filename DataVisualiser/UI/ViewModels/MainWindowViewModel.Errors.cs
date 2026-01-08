namespace DataVisualiser.UI.ViewModels;

public partial class MainWindowViewModel
{
    // ======================
    // ERROR HANDLING
    // ======================

    public void RaiseError(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            return;

        // Primary, structured error path
        ErrorOccured?.Invoke(this,
                new ErrorEventArgs
                {
                        Message = errorMessage
                });

        // Secondary, string-only path retained for compatibility
        ErrorOccurred?.Invoke(this, errorMessage);
    }

    /// <summary>
    ///     Formats a user-friendly error message for database-related failures.
    /// </summary>
    public string FormatDatabaseError(Exception ex)
    {
        return $"An error occurred while loading data: {ex.Message}";
    }
}