using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POS.Models.DTOs;
using POS.Services;

namespace POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;

        public AuthController(IAuthService authService, IJwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpPost("registro")]
        public async Task<ActionResult<UserResponseDTO>> Registro(RegisterDTO registroDto)
        {
            if (await _authService.EmailExiste(registroDto.Email))
                return BadRequest(new { message = "El email ya está registrado" });

            var user = await _authService.Registrar(registroDto);
            var token = _jwtService.GenerateToken(user.UserId, user.Email);

            var response = new UserResponseDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                CreationDate = user.CreationDate,
                Token = token
            };

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserResponseDTO>> Login(LoginDTO loginDto)
        {
            var user = await _authService.Login(loginDto);
            if (user == null)
                return Unauthorized(new { message = "Credenciales inválidas" });

            var token = _jwtService.GenerateToken(user.UserId, user.Email);

            var response = new UserResponseDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                CreationDate = user.CreationDate,
                Token = token
            };

            return Ok(response);
        }
    }
}
