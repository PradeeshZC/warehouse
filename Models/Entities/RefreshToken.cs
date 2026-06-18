#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Entities
{
    public class RefreshToken : BaseEntity
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation
        // ApplicationUser navigation is in Models/Identity/ApplicationUser to avoid Identity reference here
    }
}
