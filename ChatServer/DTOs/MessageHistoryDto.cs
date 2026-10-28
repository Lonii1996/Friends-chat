using MessagePack;


namespace ChatServer.DTOs
{
    
    public class MessageHistoryDto
    {
        
        public int Id { get; init; }
        public string Text { get; init; } = string.Empty;
        public DateTime SentAt { get; init; }     
        public int UserId { get; init; }     
        public string Username { get; init; } = string.Empty;
    }
}
