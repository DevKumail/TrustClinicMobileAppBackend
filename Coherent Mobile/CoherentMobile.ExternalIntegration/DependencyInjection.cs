using CoherentMobile.ExternalIntegration.Clients;
using CoherentMobile.ExternalIntegration.Interfaces;
using CoherentMobile.ExternalIntegration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;

namespace CoherentMobile.ExternalIntegration;

/// <summary>
/// External integration layer dependency injection configuration
/// Registers HttpClient-based third-party API clients
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddExternalIntegrationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register configuration
        services.Configure<AppointmentsApiSettings>(configuration.GetSection("AppointmentsApi"));
        services.Configure<PatientHealthApiSettings>(configuration.GetSection("PatientHealthApi"));
        services.Configure<CrmChatApiSettings>(configuration.GetSection("CrmChatApi"));

        // Register HTTP clients with typed clients pattern
        services.AddHttpClient<IHealthDataApiClient, HealthDataApiClient>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient<INotificationApiClient, NotificationApiClient>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());
            
        // Configure AppointmentApiClient with base URL from configuration
        services.AddHttpClient<IAppointmentApiClient, AppointmentApiClient>((serviceProvider, httpClient) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<AppointmentsApiSettings>>().Value;
            httpClient.BaseAddress = new Uri(settings.BaseUrl);
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5))
        .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient<IPatientHealthApiClient, PatientHealthApiClient>((serviceProvider, httpClient) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<PatientHealthApiSettings>>().Value;
            httpClient.BaseAddress = new Uri(settings.BaseUrl);
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5))
        .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient<ICrmChatApiClient, CrmChatApiClient>((serviceProvider, httpClient) =>
        {
            _ = serviceProvider.GetRequiredService<IOptions<CrmChatApiSettings>>().Value;
        })
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
