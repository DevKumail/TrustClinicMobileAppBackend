
using CoherentMobile.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CoherentMobile.Api.Services;

public class MedicationReminderScheduler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MedicationReminderScheduler> _logger;
    private readonly IConfiguration _configuration;

    public MedicationReminderScheduler(IServiceScopeFactory scopeFactory, ILogger<MedicationReminderScheduler> logger, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = _configuration.GetValue<int?>("MedicationReminders:PollIntervalSeconds") ?? 30;
        if (intervalSeconds <= 0) intervalSeconds = 30;

        var take = _configuration.GetValue<int?>("MedicationReminders:BatchSize") ?? 200;
        if (take <= 0) take = 200;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IMedicationReminderService>();

                var nowUtc = DateTime.UtcNow;
                var processed = await svc.ProcessDueRemindersAsync(nowUtc, take);

                if (processed > 0)
                {
                    _logger.LogInformation("Medication reminder scheduler processed {Count} reminder(s)", processed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Medication reminder scheduler failed");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}

