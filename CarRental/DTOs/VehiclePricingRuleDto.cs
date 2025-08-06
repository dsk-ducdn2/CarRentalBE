namespace CarRental.DTOs;

public class VehiclePricingRuleDto
{
    public Guid? VehicleId { get; set; }

    public decimal? PricePerDay { get; set; }

    public double? HolidayMultiplier { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public DateTime? ExpiryDate { get; set; }
}