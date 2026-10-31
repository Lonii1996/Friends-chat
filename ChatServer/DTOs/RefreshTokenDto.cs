using System.ComponentModel.DataAnnotations;

namespace ChatServer.DTOs
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "Передайте текущий рефреш-токен для обновления сессии")]
        public string RefreshToken { get; init; } = string.Empty;
    }
}
