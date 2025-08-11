using CarRental.Data;
using CarRental.Models;
using CarRental.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarRental.Jobs;

public class MaintenanceReminderService : BackgroundService
{
    private readonly ILogger<MaintenanceReminderService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<MaintenanceReminderOptions> _options;

    public MaintenanceReminderService(
        ILogger<MaintenanceReminderService> logger,
        IServiceProvider serviceProvider,
        IOptions<MaintenanceReminderOptions> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MaintenanceReminderService started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanAndCreateReminders(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while scanning maintenance reminders");
            }

            var delayMinutes = Math.Max(1, _options.Value.CheckIntervalMinutes);
            await Task.Delay(TimeSpan.FromMinutes(delayMinutes), stoppingToken);
        }
    }

    private async Task ScanAndCreateReminders(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notifier = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var nowUtc = DateTime.UtcNow;
        var daysBefore = Math.Max(0, _options.Value.ReminderDaysBefore);
        var windowStartUtc = nowUtc;
        var windowEndUtc = nowUtc.AddDays(daysBefore);

        // Assumption: Maintenance.Status == "1" means scheduled/pending.
        // We'll set to "REMINDER_SENT" when a reminder is created to avoid duplicates.
        var upcoming = await db.Maintenances
            .Where(m => m.Status == "SCHEDULED" && m.ScheduledDate >= windowStartUtc && m.ScheduledDate <= windowEndUtc)
            .Include(m => m.Vehicle)
            .ToListAsync(cancellationToken);

        if (upcoming.Count == 0)
        {
            _logger.LogDebug("No upcoming maintenances within {Days} days", daysBefore);
            return;
        }

        foreach (var maintenance in upcoming)
        {
            // Mark maintenance as reminder sent to avoid duplicate notifications
            maintenance.Status = "REMINDER_SENT"; 
            maintenance.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var maintenance in upcoming)
        {
            var message = $"Xe {maintenance.Vehicle?.LicensePlate} cần bảo dưỡng vào {maintenance.ScheduledDate:u} (Cty {maintenance.Vehicle?.CompanyId})";
            await notifier.NotifyMaintenanceDueAsync(message, cancellationToken);
        }
    }
}


