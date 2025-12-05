using CoherentMobile.Application.Interfaces;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.Infrastructure.Data;
using CoherentMobile.Infrastructure.Repositories;
using CoherentMobile.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CoherentMobile.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection configuration
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register Dapper Context
        services.AddSingleton<DapperContext>();

        // Register authentication repositories
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IOTPVerificationRepository, OTPVerificationRepository>();
        services.AddScoped<IQRCodeScanRepository, QRCodeScanRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IAuthAuditLogRepository, AuthAuditLogRepository>();
        
        // Register clinic information repositories (Guest Mode)
        services.AddScoped<IFacilityRepository, FacilityRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<ISpecialityRepository, SpecialityRepository>();
        
        // Register legacy repositories (if still needed)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IHealthRecordRepository, HealthRecordRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register external services
        services.AddScoped<ISMSService, SMSService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
