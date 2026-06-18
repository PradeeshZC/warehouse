#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models.DTOs
{
    public class RegisterDto
    {
        [Required]
        [StringLength(150)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        public string? FullName { get; set; }
    }
}
