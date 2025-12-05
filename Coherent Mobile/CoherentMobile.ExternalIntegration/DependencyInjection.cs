using CoherentMobile.ExternalIntegration.Clients;
using CoherentMobile.ExternalIntegration.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace CoherentMobile.ExternalIntegration;

/// <summary>
/// External integration layer dependency injection configuration
/// Registers HttpClient-based third-party API clients
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddExternalIntegrationServices(this IServiceCollection services)
    {
        // Register HTTP clients with typed clients pattern
        services.AddHttpClient<IHealthDataApiClient, HealthDataApiClient>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Set handler lifetime
            .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient<INotificationApiClient, NotificationApiClient>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());

        return services;
    }

    private static Polly.IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        // Simple retry policy - can be enhanced with Polly for production
        return Polly.Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => !msg.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
