using System.Windows.Controls;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.MainHost;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MetricSelectionComboReaderTests
{
    [Fact]
    public void GetSelectedMetricValue_ShouldPreferMetricNameOptionValue()
    {
        StaTestHelper.Run(() =>
        {
            var option = new MetricNameOption("Weight", "Body Weight");
            var combo = new ComboBox();
            combo.Items.Add(option);
            combo.SelectedItem = option;

            Assert.Equal("Weight", MetricSelectionComboReader.GetSelectedMetricValue(combo));
            Assert.Same(option, MetricSelectionComboReader.GetSelectedMetricOption(combo));
        });
    }

    [Fact]
    public void GetSelectedMetricValue_ShouldFallbackToSelectedValueThenSelectedItem()
    {
        StaTestHelper.Run(() =>
        {
            var selectedItemCombo = new ComboBox();
            selectedItemCombo.Items.Add("SelectedItem");
            selectedItemCombo.SelectedItem = "SelectedItem";

            Assert.Equal("SelectedItem", MetricSelectionComboReader.GetSelectedMetricValue(selectedItemCombo));
            Assert.Null(MetricSelectionComboReader.GetSelectedMetricOption(selectedItemCombo));
        });
    }
}
