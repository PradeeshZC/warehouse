#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        // RememberMe - true to request a longer-lived refresh token cookie
        public bool RememberMe { get; set; } = false;
    }
}
