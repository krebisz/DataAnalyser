using DataVisualiser.Models;
using DataVisualiser.ViewModels.Events;

namespace DataVisualiser.ViewModels
{
    /// <summary>
    /// Helper for validating metric data loading results.
    /// Extracted from MainWindowViewModel to improve testability.
    /// </summary>
    public static class MetricDataValidationHelper
    {
        public static bool ValidatePrimaryData(
            string metricType,
            string? primarySubtype,
            object? primaryCms,
            IEnumerable<HealthMetricData>? data1,
            EventHandler<ErrorEventArgs>? errorHandler)
        {
            if (primaryCms == null && (data1 == null || !data1.Any()))
            {
                errorHandler?.Invoke(null, new ErrorEventArgs
                {
                    Message = BuildNoDataMessage(metricType, primarySubtype, isSecondary: false)
                });
                return false;
            }
            return true;
        }

        public static bool ValidateSecondaryData(
            string metricType,
            string? secondarySubtype,
            object? secondaryCms,
            IEnumerable<HealthMetricData>? data2,
            EventHandler<ErrorEventArgs>? errorHandler)
        {
            if (secondarySubtype != null && secondaryCms == null && (data2 == null || !data2.Any()))
            {
                errorHandler?.Invoke(null, new ErrorEventArgs
                {
                    Message = BuildNoDataMessage(metricType, secondarySubtype, isSecondary: true)
                });
                return false;
            }
            return true;
        }

        private static string BuildNoDataMessage(string metricType, string? subtype, bool isSecondary)
        {
            var subtypeText = !string.IsNullOrEmpty(subtype)
                ? $" and Subtype '{subtype}'"
                : string.Empty;

            var suffix = isSecondary ? " (Chart 2)." : ".";

            return $"No data found for MetricType '{metricType}'{subtypeText} in the selected date range{suffix}";
        }
    }
}
