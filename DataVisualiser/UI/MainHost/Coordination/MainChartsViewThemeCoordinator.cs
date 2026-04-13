using DataVisualiser.UI.Theming;

namespace DataVisualiser.UI.MainHost.Coordination;

public sealed class MainChartsViewThemeCoordinator
{
    private readonly Action<Action>? _invokeOnUiThread;
    private readonly Action<string> _setToggleContent;
    private readonly AppThemeService _themeService;
    private bool _isAttached;

    public MainChartsViewThemeCoordinator(
        AppThemeService themeService,
        Action<string> setToggleContent,
        Action<Action>? invokeOnUiThread = null)
    {
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _setToggleContent = setToggleContent ?? throw new ArgumentNullException(nameof(setToggleContent));
        _invokeOnUiThread = invokeOnUiThread;
    }

    public void Attach()
    {
        if (_isAttached)
            return;

        _themeService.ThemeChanged += OnThemeChanged;
        _isAttached = true;
        UpdateToggleContent();
    }

    public void Detach()
    {
        if (!_isAttached)
            return;

        _themeService.ThemeChanged -= OnThemeChanged;
        _isAttached = false;
    }

    public void ToggleTheme()
    {
        _themeService.ToggleTheme();
    }

    public string GetToggleContent()
    {
        return _themeService.CurrentTheme == AppTheme.Light ? "Dark Theme" : "Light Theme";
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        if (_invokeOnUiThread == null)
        {
            UpdateToggleContent();
            return;
        }

        _invokeOnUiThread(UpdateToggleContent);
    }

    private void UpdateToggleContent()
    {
        _setToggleContent(GetToggleContent());
    }
}
