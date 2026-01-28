using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace DataVisualiser.UI.Helpers;

public static class ChartComboItemHelper
{
    public static void Populate(ComboBox combo, IEnumerable<(string Content, object Tag)> items)
    {
        if (combo == null)
            throw new ArgumentNullException(nameof(combo));
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        combo.Items.Clear();
        foreach (var (content, tag) in items)
            combo.Items.Add(new ComboBoxItem
            {
                    Content = content,
                    Tag = tag
            });
    }

    public static bool TrySelectByTag(ComboBox combo, Func<object?, bool> predicate)
    {
        if (combo == null)
            throw new ArgumentNullException(nameof(combo));
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        foreach (var item in combo.Items.OfType<ComboBoxItem>())
            if (predicate(item.Tag))
            {
                combo.SelectedItem = item;
                return true;
            }

        return false;
    }
}
