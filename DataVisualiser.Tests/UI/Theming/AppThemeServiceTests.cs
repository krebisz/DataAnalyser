using System.Windows;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Theming;

namespace DataVisualiser.Tests.UI.Theming;

public sealed class AppThemeServiceTests
{
    [Fact]
    public void ApplyTheme_ShouldReplaceThemeDictionary_AndUpdateCurrentTheme()
    {
        StaTestHelper.Run(() =>
        {
            var service = new AppThemeService(theme => new ResourceDictionary
            {
                ["ThemeName"] = theme.ToString()
            });
            var resources = new ResourceDictionary();

            service.ApplyTheme(AppTheme.Light, resources);

            Assert.Equal(AppTheme.Light, service.CurrentTheme);
            Assert.Contains(resources.MergedDictionaries, dictionary => Equals(dictionary["ThemeName"], AppTheme.Light.ToString()));

            service.ApplyTheme(AppTheme.Dark, resources);

            Assert.Equal(AppTheme.Dark, service.CurrentTheme);
            Assert.DoesNotContain(resources.MergedDictionaries, dictionary => Equals(dictionary["ThemeName"], AppTheme.Light.ToString()));
            Assert.Contains(resources.MergedDictionaries, dictionary => Equals(dictionary["ThemeName"], AppTheme.Dark.ToString()));
        });
    }

    [Fact]
    public void ToggleTheme_ShouldRaiseThemeChanged()
    {
        StaTestHelper.Run(() =>
        {
            var service = new AppThemeService(theme => new ResourceDictionary
            {
                ["ThemeName"] = theme.ToString()
            });
            var resources = new ResourceDictionary();
            AppTheme? observedTheme = null;

            service.ThemeChanged += (_, args) => observedTheme = args.Theme;
            service.ApplyTheme(AppTheme.Light, resources);
            service.ToggleTheme(resources);

            Assert.Equal(AppTheme.Dark, service.CurrentTheme);
            Assert.Equal(AppTheme.Dark, observedTheme);
        });
    }
}
