#nullable enable
using Microsoft.AspNetCore.Identity;

namespace Warehouse.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        // Additional profile fields can be added here
        public string? FullName { get; set; }

        // Navigation
        public ICollection<Models.Entities.RefreshToken> RefreshTokens { get; set; } = new List<Models.Entities.RefreshToken>();
    }
}
