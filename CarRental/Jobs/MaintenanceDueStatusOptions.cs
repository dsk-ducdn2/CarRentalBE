namespace CarRental.Jobs;

public class MaintenanceDueStatusOptions
{
    // How often the job checks for due maintenances (in minutes)
    public int CheckIntervalMinutes { get; set; } = 1; // default hourly
}


