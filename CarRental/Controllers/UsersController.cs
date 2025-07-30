using CarRental.Data;
using CarRental.DTOs;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users);
    }
    
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserById(Guid  id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }
    
    [Authorize]
    [HttpPut("changeStatusUser")]
    public async Task<IActionResult> ChangeStatusUser([FromBody] Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User does not exist." });
        }

        user.Status = user.Status == "0" ? "1" : "0";
        user.UpdatedAt = DateTime.Now;

        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Internal error while updating user",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }

        return Ok(new
        {
            message = "User status updated successfully.",
            user
        });
    }
    
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto request)
    {
        // Kiểm tra các trường bắt buộc (nếu cần)
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Phone))
        {
            return BadRequest(new { message = "Name, Email, Phone, Password are required." });
        }
        
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }
        
        if (_context.Users.Any(e => e.Email == request.Email && e.Id != id))
        {
            return Conflict(new { message = "Email is already in use." });
        }

        if (_context.Users.Any(e => e.Phone == request.Phone && e.Id != id))
        {
            return Conflict(new { message = "Phone is already in use." });
        }

        // Gán lại giá trị mới từ request nếu có
        user.Name = request.Name ?? user.Name;
        user.Email = request.Email ?? user.Email;
        user.Phone = request.Phone ?? user.Phone;
        if (request.CompanyId.HasValue)
            user.CompanyId = request.CompanyId.Value;

        if (request.RoleId.HasValue)
            user.RoleId = request.RoleId.Value;
        user.UpdatedAt = DateTime.UtcNow;
        
        // Nếu có thay đổi mật khâu
        if (request.Password != null)
        {
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }
        }

        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User updated successfully.",
                user
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while updating the user.",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
    
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
    
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }
    
        try
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
    
            return Ok(new { message = "User deleted successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while deleting the user.",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UpdateUserDto request)
    {
        // Kiểm tra các trường bắt buộc (nếu cần)
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Name, Email, Phone, Password are required." });
        }

        if (_context.Users.Any(e => e.Email == request.Email))
        {
            return Conflict(new { message = "Email is already in use." });
        }

        if (_context.Users.Any(e => e.Phone == request.Phone))
        {
            return Conflict(new { message = "Phone is already in use." });
        }
        
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            CompanyId = request.CompanyId,
            RoleId = request.RoleId ?? 2,
            PasswordHash = passwordHash,
            Status = "0", 
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User created successfully.",
                user
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while creating the user.",
                error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

}