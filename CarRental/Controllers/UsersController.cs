using CarRental.Data;
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
}