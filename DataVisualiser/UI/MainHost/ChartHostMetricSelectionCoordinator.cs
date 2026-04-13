using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.MainHost;

public sealed class ChartHostMetricSelectionCoordinator
{
    public enum SubtypesFollowUp
    {
        None,
        LoadDateRange,
        ApplySelectionState
    }

    public sealed record MetricTypesLoadedActions(
        Action ClearMetricTypeItems,
        Action<MetricNameOption> AddMetricTypeItem,
        Func<int> GetMetricTypeItemCount,
        Action<bool> SetApplyingSelectionSync,
        Func<IDisposable> BeginSelectionStateBatch,
        Action<int> SetSelectedMetricIndex,
        Func<string?> GetSelectedMetricValue,
        Action<string?> SetSelectedMetricType,
        Action LoadSubtypes,
        Action ClearPrimarySubtypeItems,
        Action<bool> SetPrimarySubtypeEnabled,
        Action ClearDynamicSubtypeControls);

    public sealed record MetricTypeSelectionChangedActions(
        Action<bool> SetMetricTypeChangePending,
        Action<bool> SetApplyingSelectionSync,
        Func<IDisposable> BeginSelectionStateBatch,
        Action ResetSelectionStateForMetricTypeChange,
        Action<string?> SetSelectedMetricType,
        Action ClearAllSubtypeControls,
        Action UpdateSelectedSubtypesInViewModel,
        Action LoadSubtypes);

    public sealed record SubtypesLoadedInput(
        IReadOnlyList<MetricNameOption> Subtypes,
        MetricNameOption? SelectedMetricType,
        bool IsMetricTypeChangePending,
        bool HasLoadedData,
        bool ShouldRefreshDateRangeForCurrentSelection,
        bool IsInitializing,
        int SelectedSeriesCount);

    public sealed record SubtypesLoadedActions(
        Action<bool> SetApplyingSelectionSync,
        Func<IDisposable> SuppressSelectionChanged,
        Func<IDisposable> BeginSelectionStateBatch,
        Action<IReadOnlyList<MetricNameOption>, bool, MetricNameOption?> RefreshPrimarySubtypeCombo,
        Action<IReadOnlyList<MetricNameOption>> BuildDynamicSubtypeControls,
        Action UpdateSelectedSubtypesInViewModel,
        Action<bool> SetMetricTypeChangePending);

    public void HandleMetricTypesLoaded(IReadOnlyList<MetricNameOption> metricTypes, MetricTypesLoadedActions actions)
    {
        ArgumentNullException.ThrowIfNull(metricTypes);
        ArgumentNullException.ThrowIfNull(actions);

        actions.ClearMetricTypeItems();

        var addedAllMetricType =
            metricTypes.Count > 0 &&
            !metricTypes.Any(type => string.Equals(type.Value, "(All)", StringComparison.OrdinalIgnoreCase));
        if (addedAllMetricType)
            actions.AddMetricTypeItem(new MetricNameOption("(All)", "(All)"));

        foreach (var type in metricTypes)
            actions.AddMetricTypeItem(type);

        if (actions.GetMetricTypeItemCount() > 0)
        {
            actions.SetApplyingSelectionSync(true);
            try
            {
                using var selectionBatch = actions.BeginSelectionStateBatch();
                actions.SetSelectedMetricIndex(addedAllMetricType && actions.GetMetricTypeItemCount() > 1 ? 1 : 0);
                actions.SetSelectedMetricType(actions.GetSelectedMetricValue());
            }
            finally
            {
                actions.SetApplyingSelectionSync(false);
            }

            actions.LoadSubtypes();
            return;
        }

        actions.ClearPrimarySubtypeItems();
        actions.SetPrimarySubtypeEnabled(false);
        actions.ClearDynamicSubtypeControls();
    }

    public void HandleMetricTypeSelectionChanged(string? selectedMetricType, MetricTypeSelectionChangedActions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        actions.SetMetricTypeChangePending(true);
        actions.SetApplyingSelectionSync(true);
        try
        {
            using var selectionBatch = actions.BeginSelectionStateBatch();
            actions.ResetSelectionStateForMetricTypeChange();
            actions.SetSelectedMetricType(selectedMetricType);
            actions.ClearAllSubtypeControls();
            actions.UpdateSelectedSubtypesInViewModel();
        }
        finally
        {
            actions.SetApplyingSelectionSync(false);
        }

        actions.LoadSubtypes();
    }

    public SubtypesFollowUp HandleSubtypesLoaded(SubtypesLoadedInput input, SubtypesLoadedActions actions)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(actions);

        actions.SetApplyingSelectionSync(true);
        try
        {
            using var comboSuppression = actions.SuppressSelectionChanged();
            using var selectionBatch = actions.BeginSelectionStateBatch();

            actions.RefreshPrimarySubtypeCombo(input.Subtypes, false, input.SelectedMetricType);
            actions.BuildDynamicSubtypeControls(input.Subtypes);
            actions.UpdateSelectedSubtypesInViewModel();
        }
        finally
        {
            actions.SetApplyingSelectionSync(false);
        }

        if (input.IsMetricTypeChangePending)
        {
            actions.SetMetricTypeChangePending(false);
            return SubtypesFollowUp.LoadDateRange;
        }

        if (!input.HasLoadedData && input.ShouldRefreshDateRangeForCurrentSelection)
            return SubtypesFollowUp.LoadDateRange;

        if (!input.IsInitializing && input.SelectedSeriesCount > 0)
            return SubtypesFollowUp.ApplySelectionState;

        return SubtypesFollowUp.None;
    }
}
