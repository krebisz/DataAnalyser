using System.Windows;

namespace DataVisualiser.UI.Theming;

public sealed class AppThemeService
{
    internal const string ThemeMarkerKey = "__AppTheme";
    private readonly Func<AppTheme, ResourceDictionary> _dictionaryFactory;

    public static AppThemeService Default { get; } = new();

    public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public event EventHandler<AppThemeChangedEventArgs>? ThemeChanged;

    public AppThemeService(Func<AppTheme, ResourceDictionary>? dictionaryFactory = null)
    {
        _dictionaryFactory = dictionaryFactory ?? CreateRuntimeThemeDictionary;
    }

    public void ApplyTheme(AppTheme theme, ResourceDictionary? resources = null)
    {
        var targetResources = resources ?? Application.Current?.Resources;
        if (targetResources == null)
        {
            CurrentTheme = theme;
            ThemeChanged?.Invoke(this, new AppThemeChangedEventArgs(theme));
            return;
        }

        ReplaceThemeDictionary(targetResources, theme);
        CurrentTheme = theme;
        ThemeChanged?.Invoke(this, new AppThemeChangedEventArgs(theme));
    }

    public void ToggleTheme(ResourceDictionary? resources = null)
    {
        var nextTheme = CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
        ApplyTheme(nextTheme, resources);
    }

    internal static Uri GetThemeDictionaryUri(AppTheme theme)
    {
        var path = theme == AppTheme.Dark ? "Theme.Dark.xaml" : "Theme.Light.xaml";
        return new Uri($"/DataVisualiser;component/UI/Theming/{path}", UriKind.Relative);
    }

    private void ReplaceThemeDictionary(ResourceDictionary resources, AppTheme theme)
    {
        var existingThemeDictionary = resources.MergedDictionaries
            .FirstOrDefault(dictionary => dictionary.Contains(ThemeMarkerKey));

        if (existingThemeDictionary != null)
            resources.MergedDictionaries.Remove(existingThemeDictionary);

        resources.MergedDictionaries.Add(CreateThemeDictionary(theme));
    }

    private ResourceDictionary CreateThemeDictionary(AppTheme theme)
    {
        var dictionary = _dictionaryFactory(theme);
        dictionary[ThemeMarkerKey] = theme.ToString();
        return dictionary;
    }

    private static ResourceDictionary CreateRuntimeThemeDictionary(AppTheme theme)
    {
        return new ResourceDictionary
        {
            Source = GetThemeDictionaryUri(theme)
        };
    }
}
