using System.Windows;

namespace DataVisualiser;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeWindowLayout();
    }

    private void InitializeWindowLayout()
    {
        // Pin window to top-left corner
        Left = 0;
        Top = 0;
    }
}