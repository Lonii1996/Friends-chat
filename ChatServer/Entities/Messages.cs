using System.ComponentModel.DataAnnotations;

namespace ChatServer.Entities
{
    public class Message
    {
        public int Id { get; private set; }
        [Required]
        [MaxLength(1000)]
        public string Text { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
