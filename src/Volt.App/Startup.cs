using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Volt.Core.Configuration;
using Volt.ViewModels;

namespace Volt.App;

/// <summary>
/// Application startup and DI configuration.
/// </summary>
public static class Startup
{
    /// <summary>
    /// Configures services for dependency injection.
    /// </summary>
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configuration options
        services.Configure<AppOptions>(configuration.GetSection(AppOptions.SectionName));
        services.Configure<ChatOptions>(configuration.GetSection(ChatOptions.SectionName));
        services.Configure<InferenceOptions>(configuration.GetSection(InferenceOptions.SectionName));
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        services.Configure<LoggingOptions>(configuration.GetSection(LoggingOptions.SectionName));

        // Logging
        ConfigureLogging(services, configuration);

        // ViewModels
        services.AddTransient<ChatViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ConversationListViewModel>();

        // Services will be registered here in Phase 2
        // services.AddSingleton<IChatService, ChatService>();
        // services.AddSingleton<ISettingsService, SettingsService>();
        // services.AddSingleton<IConversationStorage, JsonConversationStorage>();

        // Inference clients will be registered here in Phase 2
        // services.AddSingleton<IInferenceClientFactory, InferenceClientFactory>();
        // services.AddSingleton<IModelManager, OllamaModelManager>();
    }

    private static void ConfigureLogging(IServiceCollection services, IConfiguration configuration)
    {
        var loggingOptions = configuration
            .GetSection(LoggingOptions.SectionName)
            .Get<LoggingOptions>() ?? new LoggingOptions();

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(ParseLogLevel(loggingOptions.MinLevel));

        if (loggingOptions.WriteToConsole)
        {
            loggerConfig.WriteTo.Console();
        }

        if (loggingOptions.WriteToFile)
        {
            var logPath = Path.Combine(
                loggingOptions.ExpandedFilePath,
                "volt-.log");

            loggerConfig.WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: loggingOptions.MaxFileSizeMb * 1024 * 1024,
                retainedFileCountLimit: loggingOptions.RetainedFileCount);
        }

        Log.Logger = loggerConfig.CreateLogger();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });
    }

    private static Serilog.Events.LogEventLevel ParseLogLevel(string level)
    {
        return level.ToLowerInvariant() switch
        {
            "verbose" => Serilog.Events.LogEventLevel.Verbose,
            "debug" => Serilog.Events.LogEventLevel.Debug,
            "information" => Serilog.Events.LogEventLevel.Information,
            "warning" => Serilog.Events.LogEventLevel.Warning,
            "error" => Serilog.Events.LogEventLevel.Error,
            "fatal" => Serilog.Events.LogEventLevel.Fatal,
            _ => Serilog.Events.LogEventLevel.Information
        };
    }
}
