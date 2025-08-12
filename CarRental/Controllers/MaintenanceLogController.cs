using CarRental.Data;
using CarRental.DTOs;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaintenanceLogController : ControllerBase
{
    private readonly AppDbContext _context;

    public MaintenanceLogController(AppDbContext context)
    {
        _context = context;
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateMaintenanceLog([FromBody] MaintenanceLogDto request)
    {
        // Kiểm tra các trường bắt buộc
        if (request.MaintenanceId == Guid.Empty)
        {
            return BadRequest(new { message = "MaintenanceId are required." });
        }
        
        if (request.CreatedBy == Guid.Empty)
        {
            return BadRequest(new { message = "CreatedBy are required." });
        }

        var maintenanceLog = _context.MaintenanceLogs.FirstOrDefault(x => x.MaintenanceId == request.MaintenanceId);

        try
        {
            if (maintenanceLog != null)
            {
                maintenanceLog.Note =  request.Note;
                maintenanceLog.Action = request.Action;
                _context.MaintenanceLogs.Update(maintenanceLog);
                await _context.SaveChangesAsync();
            }
            else
            {
                var maintenanceLogNew = new MaintenanceLog
                {
                    MaintenanceId = request.MaintenanceId ?? Guid.Empty,
                    Action = request.Action,
                    Note = request.Note,
                    CreatedBy = request.CreatedBy ?? Guid.Empty,
                    CreatedAt = DateTime.UtcNow,
                };
            
                _context.MaintenanceLogs.Add(maintenanceLogNew);
                await _context.SaveChangesAsync();
            }
            return Ok(new
            {
                message = "MaintenanceLog created successfully.",
                maintenanceLog
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while creating the maintenanceLog.",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
    
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<MaintenanceLog>> GetMaintenanceLogById(Guid id)
    {
        var maintenanceLog = await _context.MaintenanceLogs.FirstOrDefaultAsync(e => e.MaintenanceId == id);

        return Ok(maintenanceLog);
    }
}