using DataVisualiser.Shared.Models;
using System.Globalization;
using System.Windows.Data;
// where NormalizationMode is defined

namespace DataVisualiser.UI.Converters;

public class NormalizationModeConverter : IValueConverter
{
    // Converts NormalizationMode -> bool (IsChecked)
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is null)
            return false;
        if (value is not NormalizationMode mode)
            return false;

        var param = parameter.ToString() ?? string.Empty;
        return string.Equals(mode.ToString(), param, StringComparison.OrdinalIgnoreCase);
    }

    // Converts back from IsChecked -> NormalizationMode (only when checked)
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter is string paramStr)
            if (Enum.TryParse(typeof(NormalizationMode), paramStr, true, out var result))
                return (NormalizationMode)result;

        return Binding.DoNothing;
    }
}
