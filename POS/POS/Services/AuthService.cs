using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using POS.Data;
using POS.Models.DTOs;
using POS.Models.Entities;

namespace POS.Services
{
    public interface IAuthService
    {
        Task<User> Registrar(RegisterDTO registroDto);
        Task<User> Login(LoginDTO loginDto);
        Task<bool> EmailExiste(string email);
    }
    public class AuthService : IAuthService
    {
        private readonly POSDbContext _context;

        public AuthService(POSDbContext context)
        {
            _context = context;
        }

        public async Task<User> Registrar(RegisterDTO registryDto)
        {
            var user = new User
            {
                Name = registryDto.Name,
                Email = registryDto.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registryDto.Password),
                IsActive = true
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
                throw new InvalidOperationException("Usuario o contraseña incorrectos.");

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> EmailExiste(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email.ToLower());
        }
    }
}
