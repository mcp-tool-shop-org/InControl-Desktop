using Microsoft.Extensions.DependencyInjection;
using InControl.Inference.Interfaces;

namespace InControl.Inference.Extensions;

/// <summary>
/// Extension methods for registering inference services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds inference services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInferenceServices(this IServiceCollection services)
    {
        // These will be implemented in Phase 2
        // services.AddSingleton<IInferenceClientFactory, InferenceClientFactory>();
        // services.AddSingleton<IModelManager, OllamaModelManager>();

        return services;
    }

    /// <summary>
    /// Adds the Ollama backend to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOllamaBackend(this IServiceCollection services)
    {
        // Will be implemented in Phase 2
        // services.AddSingleton<OllamaInferenceClient>();

        return services;
    }
}
