using Microsoft.Extensions.DependencyInjection;
using InControl.Services.Interfaces;

namespace InControl.Services.Extensions;

/// <summary>
/// Extension methods for registering application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // These will be implemented in Phase 2
        // services.AddSingleton<IChatService, ChatService>();
        // services.AddSingleton<ISettingsService, SettingsService>();
        // services.AddSingleton<IConversationStorage, JsonConversationStorage>();
        // services.AddSingleton<INavigationService, NavigationService>();

        return services;
    }
}
