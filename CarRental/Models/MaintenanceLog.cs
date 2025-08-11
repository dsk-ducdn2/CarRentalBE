using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models;

[Table("maintenance_logs")]
public class MaintenanceLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("maintenance_id")]
    public Guid MaintenanceId { get; set; }

    [Column("action")]
    public string Action { get; set; }

    [Column("note")]
    public string Note { get; set; }
    
    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    public Maintenance Maintenance { get; set; }
    
    public User User { get; set; }
}