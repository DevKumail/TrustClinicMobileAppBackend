
using CoherentMobile.Application.Interfaces;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.Infrastructure.Data;
using CoherentMobile.Infrastructure.Repositories;
using CoherentMobile.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CoherentMobile.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<DapperContext>();

        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IOTPVerificationRepository, OTPVerificationRepository>();
        services.AddScoped<IQRCodeScanRepository, QRCodeScanRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IAuthAuditLogRepository, AuthAuditLogRepository>();

        services.AddScoped<IFacilityRepository, FacilityRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<ISpecialityRepository, SpecialityRepository>();

        services.AddScoped<IChatRepository, ChatRepository>();

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();
        services.AddScoped<IMedicationReminderRepository, MedicationReminderRepository>();

        services.AddScoped<IPushNotificationSender, FcmPushNotificationSender>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IHealthRecordRepository, HealthRecordRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ISMSService, SMSService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}

