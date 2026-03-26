using System;
using System.Linq;

namespace DataVisualiser.UI.Syncfusion;

public static class SyncfusionTooltipReflectionHelper
{
    public static void DisableThirdPartyTooltip(object target)
    {
        TrySetBoolProperty(target, "ShowToolTip", false);
        TrySetBoolProperty(target, "ShowTooltip", false);
        TrySetBoolProperty(target, "EnableToolTip", false);
        TrySetBoolProperty(target, "EnableTooltip", false);
        TrySetBoolProperty(target, "ToolTipEnabled", false);
        TrySetBoolProperty(target, "TooltipEnabled", false);

        TrySetNullProperty(target, "ToolTipTemplate");
        TrySetNullProperty(target, "TooltipTemplate");
        TrySetNullProperty(target, "ToolTip");
        TrySetNullProperty(target, "Tooltip");

        TryDisableAnyTooltipBooleans(target);
        TryDisableAnyTooltipEnums(target);
        TryDisableNestedTooltipBehaviors(target);
    }

    public static bool TryExtractCategoryKey(object? source, out string key)
    {
        key = string.Empty;
        if (source == null)
            return false;

        if (source is string text && !string.IsNullOrWhiteSpace(text))
        {
            key = text;
            return true;
        }

        try
        {
            foreach (var name in new[] { "Category", "Submetric", "Group" })
            {
                var prop = source.GetType().GetProperty(name);
                if (prop == null)
                    continue;

                var value = prop.GetValue(source);
                var asText = value?.ToString();
                if (string.IsNullOrWhiteSpace(asText))
                    continue;

                key = asText;
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static void TrySetBoolProperty(object target, string propertyName, bool value)
    {
        try
        {
            var prop = target.GetType().GetProperty(propertyName);
            if (prop == null || prop.PropertyType != typeof(bool) || !prop.CanWrite)
                return;

            prop.SetValue(target, value);
        }
        catch
        {
        }
    }

    private static void TrySetNullProperty(object target, string propertyName)
    {
        try
        {
            var prop = target.GetType().GetProperty(propertyName);
            if (prop == null || !prop.CanWrite || prop.PropertyType.IsValueType)
                return;

            prop.SetValue(target, null);
        }
        catch
        {
        }
    }

    private static void TryDisableAnyTooltipBooleans(object target)
    {
        try
        {
            foreach (var prop in target.GetType().GetProperties())
            {
                if (!prop.CanWrite || prop.PropertyType != typeof(bool))
                    continue;

                if (!prop.Name.Contains("tooltip", StringComparison.OrdinalIgnoreCase))
                    continue;

                prop.SetValue(target, false);
            }
        }
        catch
        {
        }
    }

    private static void TryDisableAnyTooltipEnums(object target)
    {
        try
        {
            foreach (var prop in target.GetType().GetProperties())
            {
                if (!prop.CanWrite || !prop.PropertyType.IsEnum)
                    continue;

                if (!prop.Name.Contains("tooltip", StringComparison.OrdinalIgnoreCase) &&
                    !prop.Name.Contains("toolTip", StringComparison.OrdinalIgnoreCase))
                    continue;

                var enumType = prop.PropertyType;
                var names = Enum.GetNames(enumType);
                var disabledName =
                    names.FirstOrDefault(n => string.Equals(n, "None", StringComparison.OrdinalIgnoreCase)) ??
                    names.FirstOrDefault(n => n.Contains("None", StringComparison.OrdinalIgnoreCase)) ??
                    names.FirstOrDefault(n => n.Contains("Disable", StringComparison.OrdinalIgnoreCase)) ??
                    names.FirstOrDefault(n => n.Contains("Off", StringComparison.OrdinalIgnoreCase));

                if (disabledName == null)
                    continue;

                prop.SetValue(target, Enum.Parse(enumType, disabledName));
            }
        }
        catch
        {
        }
    }

    private static void TryDisableNestedTooltipBehaviors(object target)
    {
        try
        {
            foreach (var prop in target.GetType().GetProperties())
            {
                if (!prop.CanRead)
                    continue;

                if (!prop.Name.Contains("behavior", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!prop.Name.Contains("tooltip", StringComparison.OrdinalIgnoreCase) &&
                    !prop.Name.Contains("toolTip", StringComparison.OrdinalIgnoreCase))
                    continue;

                var nested = prop.GetValue(target);
                if (nested == null)
                    continue;

                TryDisableAnyTooltipBooleans(nested);
                TryDisableAnyTooltipEnums(nested);
                TrySetNullProperty(nested, "ToolTipTemplate");
                TrySetNullProperty(nested, "TooltipTemplate");
                TrySetNullProperty(nested, "ToolTip");
                TrySetNullProperty(nested, "Tooltip");
            }
        }
        catch
        {
        }
    }
}
