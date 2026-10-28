namespace ChatServer.DTOs;

using MessagePack;
[MessagePackObject]
public class CreateMessageDto
{
    [Key(0)]
    public string Text { get; init; } = string.Empty;
}