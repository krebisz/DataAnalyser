using System.Windows.Controls;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.Tests.Controls;

public sealed class TransformSelectionInteractionCoordinatorTests
{
    [Fact]
    public async Task HandleSelectionChangedAsync_ShouldApplySelectionAndRefreshWhenInteractive()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var coordinator = new TransformSelectionInteractionCoordinator();
            var combo = new ComboBox();
            combo.Items.Add(new MetricSeriesSelection("Weight", "body_mass", "Weight:body_mass"));
            combo.SelectedIndex = 0;

            MetricSeriesSelection? applied = null;
            var calls = new List<string>();

            await coordinator.HandleSelectionChangedAsync(
                isInitializing: false,
                isUpdatingTransformSubtypeCombos: false,
                combo,
                selection => applied = selection,
                () => calls.Add("compute"),
                () =>
                {
                    calls.Add("refresh");
                    return Task.CompletedTask;
                });

            Assert.NotNull(applied);
            Assert.Equal("body_mass", applied!.QuerySubtype);
            Assert.Equal(["compute", "refresh"], calls);
        });
    }

    [Fact]
    public async Task HandleSelectionChangedAsync_ShouldDoNothingWhileInitializing()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var coordinator = new TransformSelectionInteractionCoordinator();
            var combo = new ComboBox();
            combo.Items.Add(new MetricSeriesSelection("Weight", "body_mass", "Weight:body_mass"));
            combo.SelectedIndex = 0;
            var calls = new List<string>();

            await coordinator.HandleSelectionChangedAsync(
                isInitializing: true,
                isUpdatingTransformSubtypeCombos: false,
                combo,
                _ => calls.Add("apply"),
                () => calls.Add("compute"),
                () =>
                {
                    calls.Add("refresh");
                    return Task.CompletedTask;
                });

            Assert.Empty(calls);
        });
    }
}
