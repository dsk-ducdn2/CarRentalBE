using System;

namespace CarRental.Jobs;

public class MaintenanceReminderOptions
{
    public int CheckIntervalMinutes { get; set; } = 1;
    public int ReminderDaysBefore { get; set; } = 3;
}


