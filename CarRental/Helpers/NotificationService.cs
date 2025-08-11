using Microsoft.Extensions.Logging;

namespace CarRental.Helpers;

public interface INotificationService
{
    Task NotifyMaintenanceDueAsync(string message, CancellationToken cancellationToken = default);
}

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task NotifyMaintenanceDueAsync(string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MAINTENANCE-NOTIFY] {Message}", message);
        return Task.CompletedTask;
    }
}


