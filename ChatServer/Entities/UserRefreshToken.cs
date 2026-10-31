using System.ComponentModel.DataAnnotations;

namespace ChatServer.Entities
{
    public class UserRefreshToken
    {
        public int Id { get; init; }

        [Required]
        public string TokenHash { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;


        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
