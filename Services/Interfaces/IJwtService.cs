#nullable enable
using Warehouse.Models.Identity;

namespace Warehouse.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(ApplicationUser user, IEnumerable<System.Security.Claims.Claim> claims);
        string GenerateRefreshToken();
        System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
