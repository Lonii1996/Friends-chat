using System.ComponentModel.DataAnnotations;

namespace ChatServer.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Имя должно быть от 3 до 20 символов")]
        public string Name { get; init; }
        [Required(ErrorMessage = "Пароль обязательн")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Пароль должен быть от 3 до 20 символов")]
        public string Password { get; init; }
    }
}
