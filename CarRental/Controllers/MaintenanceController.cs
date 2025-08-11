using CarRental.Data;
using CarRental.DTOs;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaintenanceController : ControllerBase
{
    private readonly AppDbContext _context;

    public MaintenanceController(AppDbContext context)
    {
        _context = context;
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateMaintenance([FromBody] MaintenanceDto request)
    {
        // Kiểm tra các trường bắt buộc
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = "Title are required." });
        }
        
        if (request.ScheduledDate < DateTime.Now)
        {
            return BadRequest(new { message = "Scheduled date must greater than now" });
        }

        var maintenance = new Maintenance
        {
            Id = Guid.NewGuid(),
            VehicleId = request.VehicleId ?? Guid.Empty,
            Title = request.Title,
            Description = request.Description,
            ScheduledDate = request.ScheduledDate ?? DateTime.MaxValue,
            Status = "SCHEDULED",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        try
        {
            _context.Maintenances.Add(maintenance);
            await _context.SaveChangesAsync();
            
            return Ok(new
            {
                message = "Maintenance created successfully.",
                maintenance
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while creating the company.",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
    
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<Maintenance>> GetAllMaintenances()
    {
        var maintenancess = await _context.Maintenances
            .Include(e => e.Vehicle)
            .ToListAsync();

        return Ok(maintenancess);
    }
    
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMaintenance(Guid id)
    {
        var maintenance = await _context.Maintenances.FindAsync(id);
        if (maintenance == null)
        {
            return NotFound(new { message = "Maintenance not found." });
        }

        try
        {
            var maintenanceLogs = await _context.MaintenanceLogs
                .Where(e => e.MaintenanceId == id)
                .ToListAsync();

            _context.MaintenanceLogs.RemoveRange(maintenanceLogs);

            _context.Maintenances.Remove(maintenance);
            await _context.SaveChangesAsync();
            
            return Ok(new 
            { 
                message = "Maintenance deleted successfully.",
                deletedMaintenanceId = id
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while deleting the maintenance.",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
}