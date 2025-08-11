namespace CarRental.DTOs;

public class MaintenanceDto
{
    public Guid? VehicleId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? ScheduledDate { get; set; }
}