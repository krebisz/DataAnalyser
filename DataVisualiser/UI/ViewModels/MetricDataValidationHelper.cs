using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.ViewModels;

/// <summary>
///     Helper for validating metric data loading results.
///     Extracted from MainWindowViewModel to improve testability.
/// </summary>
public static class MetricDataValidationHelper
{
    public static bool ValidatePrimaryData(string metricType, string? primarySubtype, object? primaryCms, IEnumerable<MetricData>? data1, EventHandler<ErrorEventArgs>? errorHandler)
    {
        if (primaryCms == null && (data1 == null || !data1.Any()))
        {
            errorHandler?.Invoke(null,
                    new ErrorEventArgs
                    {
                            Message = BuildNoDataMessage(metricType, primarySubtype, false)
                    });
            return false;
        }

        return true;
    }

    public static bool ValidateSecondaryData(string metricType, string? secondarySubtype, object? secondaryCms, IEnumerable<MetricData>? data2, EventHandler<ErrorEventArgs>? errorHandler)
    {
        if (secondarySubtype != null && secondaryCms == null && (data2 == null || !data2.Any()))
        {
            errorHandler?.Invoke(null,
                    new ErrorEventArgs
                    {
                            Message = BuildNoDataMessage(metricType, secondarySubtype, true)
                    });
            return false;
        }

        return true;
    }

    private static string BuildNoDataMessage(string metricType, string? subtype, bool isSecondary)
    {
        var subtypeText = !string.IsNullOrEmpty(subtype) ? $" and Subtype '{subtype}'" : string.Empty;

        var suffix = isSecondary ? " (Chart 2)." : ".";

        return $"No data found for MetricType '{metricType}'{subtypeText} in the selected date range{suffix}";
    }
}