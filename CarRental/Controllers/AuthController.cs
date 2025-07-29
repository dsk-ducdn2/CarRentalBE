using CarRental.Data;
using CarRental.DTOs;
using CarRental.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CarRental.Helpers;
using CarRental.Utils;

namespace CarRental.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IAesEncryptionHelper _aesEncryptionHelper;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, ITokenService  tokenService, IAesEncryptionHelper aesEncryptionHelper, IConfiguration configuration)
        {
            _context = context;
            _tokenService = tokenService;
            _configuration = configuration;
            _aesEncryptionHelper = aesEncryptionHelper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Email and password are required." });

            var user = await _context.Users.Include(e => e.Role)
                .FirstOrDefaultAsync(u => u.Email.Trim() == request.Email.Trim());

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });
            
            if (user.Status == "0")
                return BadRequest(new { message = "Your account is not activated. Please contact admin." });

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
                return Unauthorized(new { message = "Invalid email or password." });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(ClaimTypes.Role, user.Role.Name ?? ""),
            };

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshTokenValue  = _tokenService.GenerateRefreshToken();
            var refreshTokenExpires = request.RememberMe
                ? DateTime.UtcNow.AddDays(30)
                : DateTime.UtcNow.AddDays(1);
            
            // Save refresh token to DB
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = refreshTokenExpires,
                CreatedAt = DateTime.UtcNow,
                RevokedAt = null
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            
            // Hash refresh_token
            string refreshTokenValueHash = _aesEncryptionHelper.Encrypt(refreshToken.Token);
            
            return Ok(new
            {
                user = new
                {
                    user.Id,
                    user.Email,
                    user.Name,
                    user.Phone,
                    user.CreatedAt,
                    user.UpdatedAt
                },
                access_token = accessToken,
                refresh_token = refreshTokenValueHash
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (emailExists)
                return Conflict(new { message = "Email is already in use." });

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = passwordHash,
                Name = request.Name,
                Phone = request.Phone,
                Status = "0",
                RoleId = Roles.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                user = new
                {
                    newUser.Id,
                    newUser.Email,
                    newUser.Name,
                    newUser.Phone
                }
            });
        }
        
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new { message = "Refresh token is required." });

            var refresh_token = _aesEncryptionHelper.Decrypt(request.RefreshToken);
            
            // Search refresh token in DB
            var existingToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refresh_token);

            if (existingToken == null || existingToken.RevokedAt != null || existingToken.ExpiresAt <= DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            var user = existingToken.User;

            // Create claims for access token new
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name ?? "")
            };

            var newAccessToken = _tokenService.GenerateAccessToken(claims);
            var newRefreshTokenValue = _tokenService.GenerateRefreshToken();

            // Revoke refresh token old
            existingToken.RevokedAt = DateTime.UtcNow;
            _context.RefreshTokens.Update(existingToken);
            await _context.SaveChangesAsync();

            // Create refresh token new
            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = newRefreshTokenValue,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays"))
            };

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();
            
            // Hash refresh_token
            string refreshTokenValueHash = _aesEncryptionHelper.Encrypt(newRefreshToken.Token);

            return Ok(new
            {
                access_token = newAccessToken,
                refresh_token = refreshTokenValueHash
            });
        }
        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required." });
            }

            var refresh_token = _aesEncryptionHelper.Decrypt(request.RefreshToken);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refresh_token);

            if (storedToken == null)
            {
                return NotFound(new { message = "Refresh token not found." });
            }

            // If was revoked 
            if (storedToken.RevokedAt != null)
            {
                return BadRequest(new { message = "Refresh token already revoked." });
            }

            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Logged out successfully." });
        }

    }
}
