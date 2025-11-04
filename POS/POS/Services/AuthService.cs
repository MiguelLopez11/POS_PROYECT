using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using POS.Data;
using POS.Models.DTOs;
using POS.Models.Entities;
using System.Security.Cryptography;

namespace POS.Services
{
    public interface IAuthService
    {
        Task<User> Register(RegisterDTO registroDto);
        Task<User> Login(LoginDTO loginDto);
        Task<bool> EmailExiste(string email);
        Task<User> GetUserByRefreshToken(string refreshToken);
        Task UpdateRefreshToken(int usuarioId, string refreshToken);
        Task RevokeRefreshToken(int usuarioId);
    }
    public class AuthService : IAuthService
    {
        private readonly POSDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(POSDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<User> Register(RegisterDTO registerDto)
        {
            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                IsActive = true,
                //RefreshToken = _jwtService.GenerateToken(registerDto.)
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> Login(LoginDTO loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email.ToLower() && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return null;

            // Generar nuevo refresh token en cada login
            user.RefreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            user.LastLogin = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> EmailExiste(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email.ToLower());
        }
        public async Task<User?> GetUserByRefreshToken(string refreshToken)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken &&
                                       u.RefreshTokenExpiryTime > DateTime.UtcNow &&
                                       u.IsActive);
        }
        public async Task UpdateRefreshToken(int usuarioId, string refreshToken)
        {
            var usuario = await _context.Users.FindAsync(usuarioId);
            if (usuario != null)
            {
                usuario.RefreshToken = refreshToken;
                usuario.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();
            }
        }
        public async Task RevokeRefreshToken(int usuarioId)
        {
            var usuario = await _context.Users.FindAsync(usuarioId);
            if (usuario != null)
            {
                usuario.RefreshToken = null;
                usuario.RefreshTokenExpiryTime = null;
                await _context.SaveChangesAsync();
            }
        }
    }
}
