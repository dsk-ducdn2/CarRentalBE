using CarRental.Data;
using CarRental.DTOs;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CompaniesController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Company>>> GetAllCompanies()
    {
        var companies = await _context.Companies.ToListAsync();
        return Ok(companies);
    }
    
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<Company>> GetCompanyById(Guid id)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company == null)
        {
            return NotFound(new { message = "Company not found" });
        }

        return Ok(company);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] UpdateCompanyDto request)
    {
        // Kiểm tra các trường bắt buộc
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Phone))
        {
            return BadRequest(new { message = "Name, Email, Phone are required." });
        }
    
        var company = await _context.Companies.FindAsync(id);

        if (company == null)
        {
            return NotFound(new { message = "Company not found." });
        }
    
        // Kiểm tra name trùng lặp
        if (_context.Companies.Any(c => c.Name == request.Name && c.Id != id))
        {
            return Conflict(new { message = "Company name is already in use." });
        }

        // Kiểm tra email trùng lặp
        if (_context.Companies.Any(c => c.Email == request.Email && c.Id != id))
        {
            return Conflict(new { message = "Email is already in use." });
        }

        // Kiểm tra phone trùng lặp
        if (_context.Companies.Any(c => c.Phone == request.Phone && c.Id != id))
        {
            return Conflict(new { message = "Phone is already in use." });
        }

        // Cập nhật thông tin company
        company.Name = request.Name;
        company.Email = request.Email;
        company.Phone = request.Phone;
        company.Address = request.Address ?? company.Address;
        company.UpdatedAt = DateTime.UtcNow;

        try
        {
            _context.Companies.Update(company);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Company updated successfully.",
                company
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while updating the company.",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(Guid id)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company == null)
        {
            return NotFound(new { message = "Company not found." });
        }

        // Kiểm tra xem có user nào đang thuộc company này không
        var hasUsers = await _context.Users.AnyAsync(u => u.CompanyId == id);
        if (hasUsers)
        {
            return BadRequest(new { message = "Cannot delete company. There are users associated with this company." });
        }

        try
        {
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Company deleted successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while deleting the company.",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateCompany([FromBody] UpdateCompanyDto request)
    {
        // Kiểm tra các trường bắt buộc
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Phone))
        {
            return BadRequest(new { message = "Name, Email, Phone are required." });
        }

        // Kiểm tra name trùng lặp
        if (_context.Companies.Any(c => c.Name == request.Name))
        {
            return Conflict(new { message = "Company name is already in use." });
        }

        // Kiểm tra email trùng lặp
        if (_context.Companies.Any(c => c.Email == request.Email))
        {
            return Conflict(new { message = "Email is already in use." });
        }

        // Kiểm tra phone trùng lặp
        if (_context.Companies.Any(c => c.Phone == request.Phone))
        {
            return Conflict(new { message = "Phone is already in use." });
        }

        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        try
        {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Company created successfully.",
                company
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

}