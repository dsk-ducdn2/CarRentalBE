using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models;

[Table("vehicle_status_logs")]
public class VehicleStatusLogs
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("vehicle_id")]
    public Guid VehicleId { get; set; }

    [Column("old_status")]
    public string OldStatus { get; set; }

    [Column("new_status")]
    public string NewStatus { get; set; }

    [Column("changed_by")]
    public Guid ChangedBy { get; set; }

    [Column("changed_at")]
    public DateTime ChangedAt { get; set; }
    
    public Vehicle Vehicle { get; set; }
    
    public User User { get; set; }
}