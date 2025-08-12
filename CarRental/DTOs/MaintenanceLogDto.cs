namespace CarRental.DTOs;

public class MaintenanceLogDto
{
    public Guid? MaintenanceId { get; set; }

    public string? Action { get; set; }

    public string? Note { get; set; }
    
    public Guid? CreatedBy { get; set; }
}