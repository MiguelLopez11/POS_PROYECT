using System.ComponentModel.DataAnnotations;

namespace POS.Models.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
