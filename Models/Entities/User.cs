#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Warehouse.Models.Enums;

namespace Warehouse.Models.Entities
{
    public class User : BaseEntity
    {
        [Required]
        [StringLength(150)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole RoleType { get; set; } = UserRole.Staff;

        // FK
        [ForeignKey("Role")]
        public int RoleId { get; set; }

        // Navigation
        public Role Role { get; set; } = null!;
    }
}
