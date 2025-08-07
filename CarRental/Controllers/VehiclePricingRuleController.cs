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
            var listOld = _context.VehiclePricingRules.Where(e => e.VehicleId == vehicleId && e.ExpiryDate != DateTime.MaxValue).ToList();
            _context.VehiclePricingRules.RemoveRange(listOld);
            await _context.SaveChangesAsync();
            
            foreach (var vehiclePricingRuleDto in request)
            {
                var isOverlapping = request.Where(e => e != vehiclePricingRuleDto).Any(e => e.EffectiveDate == vehiclePricingRuleDto.EffectiveDate
                                                ||  e.ExpiryDate == vehiclePricingRuleDto.ExpiryDate
                                                ||  (vehiclePricingRuleDto.EffectiveDate > e.EffectiveDate &&  vehiclePricingRuleDto.EffectiveDate < e.ExpiryDate)
                                                ||  (vehiclePricingRuleDto.ExpiryDate > e.EffectiveDate &&  vehiclePricingRuleDto.ExpiryDate < e.ExpiryDate) );

                if (isOverlapping)
                {
                    return BadRequest(new { message = "The date range overlaps with another record." });
                }

            // CASE: CREATE - Không trùng và không overlap
            var vehiclePricingRule = new VehiclePricingRule()
            {
                VehicleId = vehicleId,
                HolidayMultiplier = vehiclePricingRuleDto.HolidayMultiplier ?? 0,
                PricePerDay = vehiclePricingRuleDto.PricePerDay ?? 0,
                EffectiveDate = vehiclePricingRuleDto.EffectiveDate ?? DateTime.Now,
                ExpiryDate = vehiclePricingRuleDto.ExpiryDate ?? DateTime.Now, // Fix: was using ExpiryDate for both
            };
            
            _context.VehiclePricingRules.Add(vehiclePricingRule);
            await _context.SaveChangesAsync();
            }
            
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