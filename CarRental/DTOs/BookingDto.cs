namespace CarRental.DTOs;

public class BookingDto
{
    public Guid? VehicleId { get; set; }

    public Guid? UserId { get; set; }

    public DateTime? StartDatetime { get; set; }

    public DateTime? EndDatetime { get; set; }

    public decimal? TotalPrice { get; set; }
}