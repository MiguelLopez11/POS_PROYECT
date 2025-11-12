using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using POS.Models.DTOs;
using POS.Services;
using System.Security.Claims;

namespace POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("StrictPolicy")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IJwtService jwtService, IConfiguration configuration)
        {
            _authService = authService;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<TokenResponseDTO>> Register(RegisterDTO registerDTO)
        {
            if (await _authService.EmailExiste(registerDTO.Email))
                return BadRequest(new { message = "El email ya está registrado" });

            var usuario = await _authService.Register(registerDTO);
            var accessToken = _jwtService.GenerateToken(usuario.UserId, usuario.Email);

            var response = new AuthResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = usuario.RefreshToken!,
                ExpiresIn = DateTime.UtcNow.AddMinutes(GetAccessTokenExpirationMinutes()),
                Usuario = new UserResponseDTO
                {
                    UserId = usuario.UserId,
                    Name = usuario.Name,
                    Email = usuario.Email,
                    CreationDate = usuario.CreationDate
                }
            };

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDTO>> Login(LoginDTO loginDto)
        {
            var usuario = await _authService.Login(loginDto);
            if (usuario == null)
                return Unauthorized(new { message = "Credenciales inválidas" });

            var accessToken = _jwtService.GenerateToken(usuario.UserId, usuario.Email);

            var response = new AuthResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = usuario.RefreshToken!,
                ExpiresIn = DateTime.UtcNow.AddMinutes(GetAccessTokenExpirationMinutes()),
                Usuario = new UserResponseDTO
                {
                    UserId = usuario.UserId,
                    Name = usuario.Name,
                    Email = usuario.Email,
                    CreationDate = usuario.CreationDate
                }
            };
            return Ok(response);
        }
        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDTO>> RefreshToken(RefreshTokenDTO request)
        {
            try
            {
                // Obtener el principal del token expirado
                var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
                var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

                // Buscar usuario con el refresh token
                var user = await _authService.GetUserByRefreshToken(request.RefreshToken);

                if (user == null || user.UserId != userId)
                    return Unauthorized(new { message = "Refresh token inválido" });

                // Generar nuevos tokens
                var newAccessToken = _jwtService.GenerateToken(user.UserId, user.Email);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                // Actualizar refresh token en la base de datos
                await _authService.UpdateRefreshToken(user.UserId, newRefreshToken);

                var response = new TokenResponseDTO
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = DateTime.UtcNow.AddMinutes(GetAccessTokenExpirationMinutes())
                };

                return Ok(response);
            }
            catch (SecurityTokenException)
            {
                return Unauthorized(new { message = "Token inválido" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al renovar token", error = ex.Message });
            }
        }
        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            await _authService.RevokeRefreshToken(userId);

            return Ok(new { message = "Token revocado exitosamente" });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            await _authService.RevokeRefreshToken(userId);

            return Ok(new { message = "Logout exitoso" });
        }
        private int GetAccessTokenExpirationMinutes()
        {
            return Convert.ToInt32(_configuration["Jwt:AccessTokenExpireMinutes"] ?? "15");
        }
    }
}
