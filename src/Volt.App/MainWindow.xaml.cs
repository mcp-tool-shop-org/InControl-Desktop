using Microsoft.UI.Xaml;

namespace Volt.App;

/// <summary>
/// Main application window.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Set minimum window size
        var appWindow = this.AppWindow;
        appWindow.Title = "Volt - Local AI Chat";

        // Set window size
        appWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));
    }
}
