using CarRental.Data;
using CarRental.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarRental.Jobs;

public class MaintenanceDueStatusService : BackgroundService
{
    private readonly ILogger<MaintenanceDueStatusService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<MaintenanceDueStatusOptions> _options;

    public MaintenanceDueStatusService(
        ILogger<MaintenanceDueStatusService> logger,
        IServiceProvider serviceProvider,
        IOptions<MaintenanceDueStatusOptions> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MaintenanceDueStatusService started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateVehiclesForDueMaintenances(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating vehicle status for due maintenances");
            }

            var delayMinutes = Math.Max(1, _options.Value.CheckIntervalMinutes);
            await Task.Delay(TimeSpan.FromMinutes(delayMinutes), stoppingToken);
        }
    }

    private async Task UpdateVehiclesForDueMaintenances(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var today = DateTime.UtcNow.Date;

        // Find maintenances that are scheduled now or in the past and not yet completed
        var dueMaintenances = await db.Maintenances
            .Where(m => m.ScheduledDate.Date == today && m.Status == "SCHEDULED")
            .Include(m => m.Vehicle)
            .ToListAsync(cancellationToken);

        if (dueMaintenances.Count == 0)
        {
            _logger.LogDebug("No due maintenances to process");
            return;
        }

        foreach (var maintenance in dueMaintenances)
        {
            var vehicle = maintenance.Vehicle;
            if (vehicle == null)
            {
                continue;
            }

            var oldStatus = vehicle.Status;
            if (!string.Equals(oldStatus, "MAINTENANCE", StringComparison.OrdinalIgnoreCase))
            {
                vehicle.Status = "MAINTENANCE";
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
            }

            // Optional: mark maintenance as IN_PROGRESS
            maintenance.Status = "IN_PROGRESS";
            maintenance.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var m in dueMaintenances)
        {
            _logger.LogInformation("Vehicle {Plate} set to MAINTENANCE due to scheduled maintenance at {Date:u}", m.Vehicle?.LicensePlate, m.ScheduledDate);
        }
    }
}


