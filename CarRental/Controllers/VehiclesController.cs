using CarRental.Data;
using CarRental.DTOs;
using CarRental.Models;
using CarRental.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly AppDbContext _context;

    public VehiclesController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Vehicle>>> GetAllVehicles()
    {
        var vehicles = await _context.Vehicles.Include(e => e.Company).ToListAsync();
        return Ok(vehicles);
    }
    
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<Vehicle>> GetVehicleById(Guid id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);

        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found" });
        }

        return Ok(vehicle);
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateVehicle([FromBody] UpdateVehicleDto request)
    {
        // Kiểm tra các trường bắt buộc
        if (string.IsNullOrWhiteSpace(request.LicensePlate) || request.CompanyId == Guid.Empty || string.IsNullOrWhiteSpace(request.Brand))
        {
            return BadRequest(new { message = "LicensePlate, Company, Brand are required." });
        }

        // Kiểm tra biển số xe trùng lặp
        if (_context.Vehicles.Any(c => c.LicensePlate == request.LicensePlate))
        {
            return Conflict(new { message = "License plate is already in use." });
        }

        var vehicles = new Vehicle
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId ?? Guid.Empty,
            LicensePlate = request.LicensePlate,
            Brand = request.Brand,
            YearManufacture = request.YearManufacture,
            Status = request.Status ?? "1",
            Mileage = request.Mileage,
            PurchaseDate = request.PurchaseDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        try
        {
            _context.Vehicles.Add(vehicles);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Vehicle created successfully.",
                vehicles
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
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicle(Guid id, [FromBody] UpdateVehicleDto request)
    {
        // Kiểm tra các trường bắt buộc
        if (string.IsNullOrWhiteSpace(request.LicensePlate) ||
            request.CompanyId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.Brand))
        {
            return BadRequest(new { message = "LicensePlate, Company, Brand are required." });
        }

        // Tìm vehicle cần update
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found." });
        }

        // Kiểm tra Company có tồn tại không
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == request.CompanyId);
        if (!companyExists)
        {
            return BadRequest(new { message = "Company not found." });
        }

        // Kiểm tra biển số xe trùng lặp (exclude current vehicle)
        var duplicateLicensePlate = await _context.Vehicles
            .AnyAsync(v => v.LicensePlate == request.LicensePlate && v.Id != id);
        if (duplicateLicensePlate)
        {
            return Conflict(new { message = "License plate is already in use." });
        }

        // Update vehicle properties
        vehicle.CompanyId = request.CompanyId ?? Guid.Empty;
        vehicle.LicensePlate = request.LicensePlate;
        vehicle.Brand = request.Brand;
        vehicle.YearManufacture = request.YearManufacture;
        vehicle.Status = request.Status ?? "AVAILABLE";
        vehicle.Mileage = request.Mileage;
        vehicle.PurchaseDate = request.PurchaseDate;
        vehicle.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();

            // Load vehicle with company for response
            var updatedVehicle = await _context.Vehicles
                .Include(v => v.Company)
                .FirstOrDefaultAsync(v => v.Id == id);

            return Ok(new
            {
                message = "Vehicle updated successfully.",
                vehicle = updatedVehicle
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while updating the vehicle.",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicle(Guid id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found." });
        }

        // Business logic validation
        if (vehicle.Status == "RENTED")
        {
            return BadRequest(new { message = "Cannot delete vehicle that is currently rented." });
        }

        try
        {
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                message = "Vehicle deleted successfully.",
                deletedVehicleId = id
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while deleting the vehicle.",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
}
