using CarRental.Data;
using CarRental.DTOs;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly AppDbContext _context;

    public BookingController(AppDbContext context)
    {
        _context = context;
    }
    
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Booking>>> GetAllBookings()
    {
        var bookings = await _context.Bookings.Include(e => e.Vehicle).Include(e => e.User).ToListAsync();
        return Ok(bookings);
    }
    
    [Authorize]
    [HttpGet("booked-dates/{vehicleId}")]
    public async Task<IActionResult> GetBookedDates([FromRoute] Guid vehicleId)
    {
        if (vehicleId == Guid.Empty)
        {
            return BadRequest(new { message = "vehicleId is required." });
        }

        var bookings = await _context.Bookings
            .Where(b => b.VehicleId == vehicleId)
            .ToListAsync();

        var uniqueDates = new HashSet<DateTime>();

        foreach (var booking in bookings)
        {
            var startDay = booking.StartDatetime.Date;
            var endDay = booking.EndDatetime.Date;
            for (var day = startDay; day <= endDay; day = day.AddDays(1))
            {
                uniqueDates.Add(day);
            }
        }

        var result = uniqueDates
            .OrderBy(d => d)
            .Select(d => d.ToString("yyyy-MM-dd"))
            .ToList();

        return Ok(result);
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] BookingDto request)
    {
        // Kiểm tra các trường bắt buộc
        if (request.VehicleId == null)
        {
            return BadRequest(new { message = "Vehicle are required." });
        }
        
        // Validate maintenance overlap by date window
        if (!request.StartDatetime.HasValue || !request.EndDatetime.HasValue)
        {
            return BadRequest(new { message = "StartDatetime and EndDatetime are required." });
        }
        
        if (request.StartDatetime > request.EndDatetime)
        {
            return BadRequest(new { message = "EndDatetime date must greater than startDatetime." });
        }
        
        if (request.TotalPrice == 0)
        {
            return BadRequest(new { message = "Price must greater than 0." });
        }

        var hasMaintenanceConflict = await _context.Maintenances
            .AnyAsync(m => m.VehicleId == (request.VehicleId ?? Guid.Empty)
                           && (m.ScheduledDate >= request.StartDatetime && m.ScheduledDate <= request.EndDatetime)
                           && m.Status != "FINISHED");

        if (hasMaintenanceConflict)
        {
            return BadRequest(new { message = "The time of booking the car coincides with the car's maintenance schedule." });
        }

        // Validate overlap with existing bookings for the same vehicle
        var hasBookingConflict = await _context.Bookings
            .AnyAsync(b => b.VehicleId == (request.VehicleId ?? Guid.Empty)
                           && b.StartDatetime < request.EndDatetime.Value
                           && b.EndDatetime > request.StartDatetime.Value);

        if (hasBookingConflict)
        {
            return BadRequest(new { message = "The booking time overlaps with another booking for this vehicle." });
        }

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            VehicleId = request.VehicleId ?? Guid.Empty,
            StartDatetime = request.StartDatetime ?? DateTime.Now,
            EndDatetime = request.EndDatetime ?? DateTime.Now,
            TotalPrice = request.TotalPrice,
            UserId = request.UserId ?? Guid.Empty,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        try
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            
            return Ok(new
            {
                message = "Booking created successfully.",
                booking
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while creating the booking.",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
    
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<Booking>> GetBookingById(Guid id)
    {
        var booking = await _context.Bookings
            .Include(v => v.Vehicle).Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (booking == null)
        {
            return NotFound(new { message = "Booking not found" });
        }

        return Ok(booking);
    }
    
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBooking(Guid id, [FromBody] BookingDto request)
    {
        // Kiểm tra các trường bắt buộc
        if (request.TotalPrice < 0)
        {
            return BadRequest(new { message = "TotalPrice must be greater than 0." });
        }
        
        var booking = _context.Bookings.FirstOrDefault(b => b.Id == id);
        
        if (booking == null)
        {
            return NotFound(new { message = "Booking not found." });
        }
    
        // Cập nhật thông tin company
        booking.TotalPrice = request.TotalPrice;

        try
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Booking updated successfully.",
                booking
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while updating the booking.",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
    
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBooking(Guid id)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking == null)
        {
            return NotFound(new { message = "Booking not found." });
        }

        try
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Booking deleted successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while deleting the Booking.",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
}