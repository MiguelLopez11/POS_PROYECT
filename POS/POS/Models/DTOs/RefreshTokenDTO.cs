using Microsoft.Build.Framework;

namespace POS.Models.DTOs
{
    public class RefreshTokenDTO
    {
        [Required]
        public required string AccessToken { get; set; }

        [Required]
        public required string RefreshToken { get; set; }
    }
}
