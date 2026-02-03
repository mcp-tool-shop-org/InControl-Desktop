using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

namespace InControl.App;

/// <summary>
/// Application entry point and DI container host.
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets the application's service provider.
    /// </summary>
    public static IServiceProvider Services => ((App)Current)._host!.Services;

    /// <summary>
    /// Gets a service from the DI container.
    /// </summary>
    public static T GetService<T>() where T : class => Services.GetRequiredService<T>();

    /// <summary>
    /// The main window.
    /// </summary>
    public static MainWindow? MainWindow { get; private set; }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Initialize DI container after XAML framework is fully initialized
        _host = CreateHostBuilder().Build();
        await _host.StartAsync();

        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Configuration is set up in Startup
                Startup.ConfigureServices(services, context.Configuration);
            });
    }
}
