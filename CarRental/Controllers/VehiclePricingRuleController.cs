using CarRental.Data;
using CarRental.DTOs;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclePricingRuleController : ControllerBase
{
    private readonly AppDbContext _context;

    public VehiclePricingRuleController(AppDbContext context)
    {
        _context = context;
    }
    
    [Authorize]
    [HttpGet("{vehicleId}")]
    public async Task<ActionResult<VehiclePricingRule>> GetVehiclePricingRuleByVehicleId(Guid vehicleId)
    {
        var vehiclePriceRule = await _context.VehiclePricingRules
            .Include(e => e.Vehicle)
            .Where(v => v.VehicleId == vehicleId && v.ExpiryDate != DateTime.MaxValue).ToListAsync();

        return Ok(vehiclePriceRule);
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateVehiclePricingRule(Guid vehicleId, [FromBody] List<VehiclePricingRuleDto> request)
    {
        try
        {
            // Validate request
            foreach (var vehiclePricingRuleDto in request)
            {
                if (!vehiclePricingRuleDto.EffectiveDate.HasValue || !vehiclePricingRuleDto.ExpiryDate.HasValue)
                {
                    return BadRequest(new { message = "EffectiveDate and ExpiryDate are required for each pricing rule." });
                }

                if (vehiclePricingRuleDto.EffectiveDate > vehiclePricingRuleDto.ExpiryDate)
                {
                    return BadRequest(new { message = "EffectiveDate must be earlier than or equal to ExpiryDate." });
                }

                // Check overlap within the request payload
                var isOverlapping = request.Where(e => e != vehiclePricingRuleDto).Any(e =>
                    e.EffectiveDate == vehiclePricingRuleDto.EffectiveDate ||
                    e.ExpiryDate == vehiclePricingRuleDto.ExpiryDate ||
                    (vehiclePricingRuleDto.EffectiveDate > e.EffectiveDate && vehiclePricingRuleDto.EffectiveDate < e.ExpiryDate) ||
                    (vehiclePricingRuleDto.ExpiryDate > e.EffectiveDate && vehiclePricingRuleDto.ExpiryDate < e.ExpiryDate)
                );

                if (isOverlapping)
                {
                    return BadRequest(new { message = "The date range overlaps with another record in the request." });
                }

                var effective = vehiclePricingRuleDto.EffectiveDate.Value;
                var expiry = vehiclePricingRuleDto.ExpiryDate.Value;

                // Check overlap with existing bookings for the same vehicle
                var hasBookingConflict = await _context.Bookings.AnyAsync(b =>
                    b.VehicleId == vehicleId &&
                    b.StartDatetime < expiry &&
                    b.EndDatetime > effective);

                if (hasBookingConflict)
                {
                    return BadRequest(new { message = "The selected date range overlaps with existing bookings for this vehicle." });
                }

                // Check overlap with maintenance schedule (exclude finished)
                var hasMaintenanceConflict = await _context.Maintenances.AnyAsync(m =>
                    m.VehicleId == vehicleId &&
                    m.Status != "FINISHED" &&
                    m.ScheduledDate >= effective.Date &&
                    m.ScheduledDate <= expiry.Date);

                if (hasMaintenanceConflict)
                {
                    return BadRequest(new { message = "The selected date range overlaps with the vehicle's maintenance schedule." });
                }
            }

            // Passed validation: replace existing non-default rules and insert new ones
            var listOld = _context.VehiclePricingRules.Where(e => e.VehicleId == vehicleId && e.ExpiryDate != DateTime.MaxValue).ToList();
            _context.VehiclePricingRules.RemoveRange(listOld);

            foreach (var vehiclePricingRuleDto in request)
            {
                var vehiclePricingRule = new VehiclePricingRule()
                {
                    VehicleId = vehicleId,
                    HolidayMultiplier = vehiclePricingRuleDto.HolidayMultiplier ?? 0,
                    PricePerDay = vehiclePricingRuleDto.PricePerDay ?? 0,
                    EffectiveDate = vehiclePricingRuleDto.EffectiveDate ?? DateTime.Now,
                    ExpiryDate = vehiclePricingRuleDto.ExpiryDate ?? DateTime.Now,
                };

                _context.VehiclePricingRules.Add(vehiclePricingRule);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Vehicle pricing rule created successfully.",
                action = "created"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while processing the vehicle pricing rule.",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
}