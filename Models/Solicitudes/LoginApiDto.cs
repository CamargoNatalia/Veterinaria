
using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Models.DTOs
{
    public class LoginApiDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
