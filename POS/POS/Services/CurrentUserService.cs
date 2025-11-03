using POS.Models.Entities;

namespace POS.Services
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? Email { get; }    // ahora anulable
        User? User { get; }       // ahora anulable
        bool IsAuthenticated { get; }
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?.User?
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                return int.TryParse(userId, out var id) ? id : null;
            }
        }

        public string? Email => _httpContextAccessor.HttpContext?.User?
            .FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        public User? User => _httpContextAccessor.HttpContext?.Items["User"] as User;

        public bool IsAuthenticated => UserId.HasValue;
    }
}
