using System.Windows.Controls;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.Tests.Controls;

public sealed class SubtypeSelectorManagerTests
{
    [Fact]
    public void ReplacePrimaryItems_AfterMetricTypeChange_ClearsDynamicStateAndSelectsFirstNewSubtype()
    {
        StaTestHelper.Run(() =>
        {
            var panel = new StackPanel();
            var primaryCombo = new ComboBox
            {
                DisplayMemberPath = "Display",
                SelectedValuePath = "Value"
            };
            panel.Children.Add(new Label());
            panel.Children.Add(primaryCombo);
            panel.Children.Add(new Button
            {
                Content = "Add Subtype"
            });

            var manager = new SubtypeSelectorManager(panel, primaryCombo);
            var oldMetric = new MetricNameOption("OldMetric", "OldMetric");
            var oldSubtypes = new[]
            {
                new MetricNameOption("OldA", "Old A"),
                new MetricNameOption("OldB", "Old B")
            };

            manager.ReplacePrimaryItems(oldSubtypes, oldMetric, false);
            manager.AddSubtypeCombo(oldSubtypes, oldMetric);

            manager.ClearAllSubtypeControls();

            var newMetric = new MetricNameOption("NewMetric", "NewMetric");
            var newSubtypes = new[]
            {
                new MetricNameOption("NewA", "New A"),
                new MetricNameOption("NewB", "New B")
            };

            manager.ReplacePrimaryItems(newSubtypes, newMetric, false);

            var selections = manager.GetSelectedSeries();

            Assert.Single(manager.GetActiveCombos());
            Assert.Single(selections);
            Assert.Equal("NewMetric", selections[0].MetricType);
            Assert.Equal("NewA", selections[0].QuerySubtype);
            Assert.Same(newMetric, manager.GetMetricTypeForCombo(primaryCombo));
        });
    }

    [Fact]
    public void EnsurePrimarySelection_AllowsDynamicSubtypeAdditionWithoutExplicitPrimaryClick()
    {
        StaTestHelper.Run(() =>
        {
            var panel = new StackPanel();
            var primaryCombo = new ComboBox
            {
                DisplayMemberPath = "Display",
                SelectedValuePath = "Value"
            };
            panel.Children.Add(new Label());
            panel.Children.Add(primaryCombo);
            panel.Children.Add(new Button
            {
                Content = "Add Subtype"
            });

            var manager = new SubtypeSelectorManager(panel, primaryCombo);
            var metric = new MetricNameOption("MetricA", "MetricA");
            var subtypes = new[]
            {
                new MetricNameOption("SubA", "Subtype A"),
                new MetricNameOption("SubB", "Subtype B")
            };

            manager.ReplacePrimaryItems(subtypes, metric, false);
            primaryCombo.SelectedItem = null;

            manager.EnsurePrimarySelection();
            manager.AddSubtypeCombo(subtypes, metric);

            var selections = manager.GetSelectedSeries();

            Assert.Equal(2, selections.Count);
            Assert.Equal("SubA", selections[0].QuerySubtype);
            Assert.Equal("SubA", selections[1].QuerySubtype);
        });
    }

    [Fact]
    public void InternalSelectionMutations_ShouldNotRaiseSubtypeSelectionChanged()
    {
        StaTestHelper.Run(() =>
        {
            var panel = new StackPanel();
            var primaryCombo = new ComboBox
            {
                DisplayMemberPath = "Display",
                SelectedValuePath = "Value"
            };
            panel.Children.Add(new Label());
            panel.Children.Add(primaryCombo);
            panel.Children.Add(new Button
            {
                Content = "Add Subtype"
            });

            var manager = new SubtypeSelectorManager(panel, primaryCombo);
            var metric = new MetricNameOption("MetricA", "MetricA");
            var subtypes = new[]
            {
                new MetricNameOption("SubA", "Subtype A"),
                new MetricNameOption("SubB", "Subtype B")
            };
            var changeCount = 0;
            manager.SubtypeSelectionChanged += (_, _) => changeCount++;

            manager.ReplacePrimaryItems(subtypes, metric, false);
            manager.EnsurePrimarySelection();
            manager.AddSubtypeCombo(subtypes, metric);
            manager.ClearAllSubtypeControls();

            Assert.Equal(0, changeCount);
        });
    }
}
