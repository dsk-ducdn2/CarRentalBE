using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models;

[Table("vehicle_pricing_rules")]
public class VehiclePricingRule
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("vehicle_id")]
    public Guid VehicleId { get; set; }

    [Column("price_per_day")]
    public decimal PricePerDay { get; set; }

    [Column("holiday_multiplier")]
    public double HolidayMultiplier { get; set; } = 1.0;

    [Column("effective_date")]
    public DateTime EffectiveDate { get; set; }

    [Column("expiry_date")]
    public DateTime ExpiryDate { get; set; }
    
    [Column("deleted_at")]
    public DateTime DeletedAt { get; set; }
    
    public Vehicle Vehicle { get; set; }
}