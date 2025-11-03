using System.ComponentModel.DataAnnotations;

namespace POS.Models.DTOs
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
    public class RegisterDTO
    {
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(6)]
        public required string Password { get; set; }
    }

    public class UserResponseDTO
    {
        public int UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public DateTime CreationDate { get; set; }
        public required string Token { get; set; }
    }
}
