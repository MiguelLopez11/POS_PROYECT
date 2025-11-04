using POS.Models.DTOs;

public class AuthResponseDTO
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime ExpiresIn { get; set; }
    public required UserResponseDTO Usuario { get; set; }
}