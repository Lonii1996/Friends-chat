using ChatServer.Interfaces;
using ChatServer.Context;
using ChatServer.DTOs;
using Microsoft.EntityFrameworkCore;
using ChatServer.Entities;


namespace ChatServer.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;

        public MessageService(ApplicationDbContext context)
        {  _context = context; }



        public async Task<List<MessageHistoryDto>> GetHistoryAsync(int? cursorMessageId ,int count)
        {
            var query = _context.Messages.AsQueryable();

            if (cursorMessageId.HasValue) 
            {
                query = query.Where(m => m.Id <  cursorMessageId.Value);
            }

            var messages = await query
                
                .OrderByDescending(m => m.SentAt)
                .Take(count)
                .Select(m => new MessageHistoryDto
                {
                    Id = m.Id,
                    Text = m.Text,
                    SentAt = m.SentAt,
                    UserId = m.UserId,
                    Username = m.User != null ? m.User.Name : "Пользователь"
                })
                .ToListAsync();

            messages.Reverse();
            return messages;


           
        }

        public async Task<SendMessageDto?> SaveMessageAsync(int userId, CreateMessageDto createMessageDto)
        {
            if (string.IsNullOrWhiteSpace(createMessageDto.Text)) 
            {
                throw new ArgumentException("Текст сообщения не может быть пустым или состоять из пробелов.");
            }


            var userName = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync();




            var newMessage = new Message
            {

                Text = createMessageDto.Text,
                UserId = userId,
                SentAt = DateTime.UtcNow,
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();





            return new SendMessageDto
            {

                Id = newMessage.Id,
                UserName = userName!,
                Text = newMessage.Text,
                SentAt = newMessage.SentAt
            };
        }
    }
}



