namespace DataVisualiser.UI.Theming;

public sealed class AppThemeChangedEventArgs : EventArgs
{
    public AppThemeChangedEventArgs(AppTheme theme)
    {
        Theme = theme;
    }

    public AppTheme Theme { get; }
}
