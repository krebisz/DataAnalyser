using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DataVisualiser.UI.Charts.Presentation;

internal interface ISelectableGridRow
{
    bool IsIncluded { get; set; }
}

internal sealed class SelectableGridRowToggleCoordinator
{
    private int? _lastClickedIndex;

    public bool HandlePreviewMouseLeftButtonDown(DataGrid grid, MouseButtonEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(e);

        var row = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
        if (row?.Item is not ISelectableGridRow selectableRow)
            return false;

        var clickedIndex = grid.ItemContainerGenerator.IndexFromContainer(row);
        if (clickedIndex < 0)
            return false;

        if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift && _lastClickedIndex.HasValue)
        {
            var rows = GetSelectableRows(grid);
            ApplyRange(rows, _lastClickedIndex.Value, clickedIndex, !selectableRow.IsIncluded);
        }
        else
        {
            selectableRow.IsIncluded = !selectableRow.IsIncluded;
        }

        _lastClickedIndex = clickedIndex;
        grid.SelectedItem = row.Item;
        e.Handled = true;
        return true;
    }

    public bool HandleKeyDown(DataGrid grid, KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(e);

        if (e.Key is not Key.Space and not Key.Enter)
            return false;

        var selectedRows = GetSelectedRows(grid);
        if (selectedRows.Count == 0)
            return false;

        ToggleRows(selectedRows);
        e.Handled = true;
        return true;
    }

    public void Reset()
    {
        _lastClickedIndex = null;
    }

    internal static void ApplyRange(
        IReadOnlyList<ISelectableGridRow> rows,
        int anchorIndex,
        int targetIndex,
        bool targetState)
    {
        if (rows.Count == 0)
            return;

        var start = Math.Max(0, Math.Min(anchorIndex, targetIndex));
        var end = Math.Min(rows.Count - 1, Math.Max(anchorIndex, targetIndex));
        for (var index = start; index <= end; index++)
            rows[index].IsIncluded = targetState;
    }

    internal static bool ToggleRows(IReadOnlyList<ISelectableGridRow> rows)
    {
        if (rows.Count == 0)
            return false;

        var targetState = false;
        foreach (var row in rows)
        {
            if (!row.IsIncluded)
            {
                targetState = true;
                break;
            }
        }

        foreach (var row in rows)
            row.IsIncluded = targetState;

        return true;
    }

    private static IReadOnlyList<ISelectableGridRow> GetSelectableRows(DataGrid grid)
    {
        var rows = new List<ISelectableGridRow>();
        foreach (var item in grid.Items)
        {
            if (item is ISelectableGridRow row)
                rows.Add(row);
        }

        return rows;
    }

    private static IReadOnlyList<ISelectableGridRow> GetSelectedRows(DataGrid grid)
    {
        var rows = new List<ISelectableGridRow>();
        foreach (var item in grid.SelectedItems)
        {
            if (item is ISelectableGridRow row)
                rows.Add(row);
        }

        return rows;
    }

    private static T? FindVisualParent<T>(DependencyObject? source) where T : DependencyObject
    {
        while (source != null)
        {
            if (source is T typed)
                return typed;

            source = VisualTreeHelper.GetParent(source);
        }

        return null;
    }
}
