using System.Windows;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.Theming;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewThemeCoordinatorTests
{
    [Fact]
    public void AttachAndToggle_ShouldUpdateButtonContentAndTheme()
    {
        var resources = new ResourceDictionary();
        var themeService = new AppThemeService(theme => new ResourceDictionary());
        var contentValues = new List<string>();
        var coordinator = new MainChartsViewThemeCoordinator(themeService, contentValues.Add);

        coordinator.Attach();
        Assert.Equal("Dark Theme", contentValues[^1]);

        themeService.ApplyTheme(AppTheme.Dark, resources);
        Assert.Equal("Light Theme", contentValues[^1]);

        coordinator.ToggleTheme();
        Assert.Equal(AppTheme.Light, themeService.CurrentTheme);

        coordinator.Detach();
    }
}
