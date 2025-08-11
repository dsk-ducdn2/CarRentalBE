using CarRental.Data;
using CarRental.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarRental.Jobs;

public class MaintenanceAutoFinishService : BackgroundService
{
    private readonly ILogger<MaintenanceAutoFinishService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<MaintenanceAutoFinishOptions> _options;

    public MaintenanceAutoFinishService(
        ILogger<MaintenanceAutoFinishService> logger,
        IServiceProvider serviceProvider,
        IOptions<MaintenanceAutoFinishOptions> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MaintenanceAutoFinishService started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MarkFinishedForYesterday(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while auto-finishing maintenances");
            }

            var delayMinutes = Math.Max(1, _options.Value.CheckIntervalMinutes);
            await Task.Delay(TimeSpan.FromMinutes(delayMinutes), stoppingToken);
        }
    }

    private async Task MarkFinishedForYesterday(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var yesterday =  DateTime.UtcNow.Date.AddDays(-1);

        var toFinish = await db.Maintenances
            .Where(m => m.ScheduledDate.Date == yesterday
                        && m.Status == "IN_PROGRESS")
            .Include(m => m.Vehicle)
            .ToListAsync(cancellationToken);

        if (toFinish.Count == 0)
        {
            _logger.LogDebug("No maintenances to auto-finish");
            return;
        }

        foreach (var m in toFinish)
        {
            var vehicle = m.Vehicle;
            if (vehicle == null)
            {
                continue;
            }

            var oldStatus = vehicle.Status;
            
            vehicle.Status = "AVAILABLE";
            vehicle.UpdatedAt = DateTime.UtcNow;

            var log = new VehicleStatusLogs
            {
                VehicleId = vehicle.Id,
                OldStatus = oldStatus,
                NewStatus = vehicle.Status,
                ChangedBy = new Guid("e2f47008-b82a-4a44-adbb-705e9a069137"),
                ChangedAt = DateTime.UtcNow
            };
            await db.VehicleStatusLogs.AddAsync(log, cancellationToken);
            
            m.Status = "FINISHED";
            m.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var m in toFinish)
        {
            _logger.LogInformation("Maintenance {Id} marked FINISHED (scheduled {Date:u})", m.Id, m.ScheduledDate);
        }
    }
}


