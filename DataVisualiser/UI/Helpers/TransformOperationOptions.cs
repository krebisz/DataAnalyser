using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace DataVisualiser.UI.Helpers;

public static class TransformOperationOptions
{
    public static void Populate(ComboBox comboBox)
    {
        Populate(comboBox, new DefaultTransformOperationProvider());
    }

    public static void Populate(ComboBox comboBox, ITransformOperationProvider provider)
    {
        if (comboBox == null)
            throw new ArgumentNullException(nameof(comboBox));
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        comboBox.Items.Clear();

        foreach (var (content, tag) in provider.GetOperations())
            comboBox.Items.Add(new ComboBoxItem
            {
                    Content = content,
                    Tag = tag
            });
    }
}
