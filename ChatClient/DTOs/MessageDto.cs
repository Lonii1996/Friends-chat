
using MessagePack;

namespace ChatClient.DTOs;

[MessagePackObject]
public class MessageDto
{
    [Key(0)]
    public int Id { get; init; }
    [Key(1)]
    public string UserName { get; init; }
    [Key(2)]
    public string Text { get; init; } = string.Empty;
    [Key(3)]
    public DateTime SentAt { get; init; }
}