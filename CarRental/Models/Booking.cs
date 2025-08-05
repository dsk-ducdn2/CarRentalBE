using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models;

[Table("bookings")]
public class Booking
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("vehicle_id")]
    public Guid VehicleId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("start_datetime")]
    public DateTime StartDatetime { get; set; }

    [Column("end_datetime")]
    public DateTime EndDatetime { get; set; }

    [Column("status")]
    public string Status { get; set; } = "PENDING";

    [Column("total_price")]
    public decimal? TotalPrice { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public Vehicle Vehicle { get; set; } = null!;

    public User User { get; set; } = null!;
}