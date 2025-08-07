namespace CarRental.DTOs;

public class UpdateVehicleDto
{
    public Guid? CompanyId { get; set; }
    public string? LicensePlate { get; set; }
    public string? Brand { get; set; }
    public int? YearManufacture { get; set; }
    public string? Status { get; set; }
    public int? Mileage { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PricePerDay { get; set; }
    public Guid? UserId { get; set; }
}