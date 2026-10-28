using Microsoft.AspNetCore.Identity.Data;
using ChatServer.DTOs;

namespace ChatServer.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterDto model);
        Task<AuthResult> LoginAsync(LoginDto model);
    }

    public class AuthResult 
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? Token { get; set; }
    }
}
