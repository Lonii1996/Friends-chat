using ChatServer.DTOs;

namespace ChatServer.Interfaces
{
    public interface IMessageService
    {
        Task<List<MessageHistoryDto>> GetHistoryAsync(int? cursorMessageId ,int count);

        Task<SendMessageDto?> SaveMessageAsync(int userId, CreateMessageDto createMessageDto);
    }
}
