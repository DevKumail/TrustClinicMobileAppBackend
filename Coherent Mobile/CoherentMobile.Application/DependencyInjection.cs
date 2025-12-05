using CoherentMobile.Application.Interfaces;
using CoherentMobile.Application.Services;
using CoherentMobile.Application.Services.Helpers;
using CoherentMobile.Application.Validators;
using CoherentMobile.Application.Validators.Auth;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CoherentMobile.Application;

/// <summary>
/// Application layer dependency injection configuration
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IHealthRecordService, HealthRecordService>();
        services.AddScoped<IClinicInfoService, ClinicInfoService>();

        // Register helpers
        services.AddScoped<JwtTokenGenerator>();

        // Register FluentValidation validators
        services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
        services.AddValidatorsFromAssemblyContaining<VerifyInformationRequestValidator>();

        return services;
    }
}
