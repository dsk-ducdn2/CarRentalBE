using System.Security.Claims;

namespace CarRental.Helpers;

public interface ITokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    bool ValidateAccessToken(string token);
}