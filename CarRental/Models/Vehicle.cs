using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models;

[Table("vehicles")]
public class Vehicle
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("company_id")]
    public Guid CompanyId { get; set; }

    [Column("license_plate")]
    public string LicensePlate { get; set; } = string.Empty;

    [Column("brand")]
    public string? Brand { get; set; }

    [Column("year_manufacture")]
    public int? YearManufacture { get; set; }

    [Column("status")]
    public string Status { get; set; } = "AVAILABLE";
    
    [Column("mileage")]
    public int? Mileage { get; set; }

    [Column("purchase_date")]
    public DateTime? PurchaseDate { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public Company Company { get; set; } = null!;
}