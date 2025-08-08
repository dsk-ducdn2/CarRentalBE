using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehicleStatusLogsController : ControllerBase
{
    private readonly AppDbContext _context;

    public VehicleStatusLogsController(AppDbContext context)
    {
        _context = context;
    }
    
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<VehicleStatusLogs>> GetAllVehicleStatusLogs()
    {
        var vehicleStatusLogs = await _context.VehicleStatusLogs
            .Include(e => e.Vehicle).ThenInclude(e => e.Company)
            .Include(e => e.User).ToListAsync();

        return Ok(vehicleStatusLogs);
    }
}