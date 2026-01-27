
using CoherentMobile.Application.Interfaces;
using CoherentMobile.Application.Services;
using CoherentMobile.Application.Services.Helpers;
using CoherentMobile.Application.Validators;
using CoherentMobile.Application.Validators.Auth;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CoherentMobile.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IHealthRecordService, HealthRecordService>();
        services.AddScoped<IClinicInfoService, ClinicInfoService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDeviceTokenService, DeviceTokenService>();
        services.AddScoped<IMedicationReminderService, MedicationReminderService>();
        services.AddScoped<IPromotionService, PromotionService>();
        services.AddScoped<IPatientEducationService, PatientEducationService>();

        services.AddScoped<JwtTokenGenerator>();

        services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
        services.AddValidatorsFromAssemblyContaining<VerifyInformationRequestValidator>();

        return services;
    }
}

