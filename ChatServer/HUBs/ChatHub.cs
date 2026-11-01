using ChatServer.Context;
using ChatServer.DTOs;
using ChatServer.Entities;
using ChatServer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;


namespace ChatServer.HUBs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IMessageService messageService, ILogger<ChatHub> logger) 
        {
            _messageService = messageService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Аноним";

            _logger.LogInformation("SignalR: Пользователь {Username} (Id: {UserId}) успешно подключился. ConnectionId: {ConnectionId}",
                userName, userId, Context.ConnectionId);

            await base.OnConnectedAsync();
        }



        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Аноним";

            if (exception != null)
            {
                _logger.LogWarning(exception, "SignalR: Пользователь {Username} (Id: {UserId}) отключился из-за сетевого сбоя.", userName, userId);
            }
            else
            {
                _logger.LogInformation("SignalR: Пользователь {Username} (Id: {UserId}) штатно разорвал соединение.", userName, userId);
            }

            await base.OnDisconnectedAsync(exception);
        }





        public async Task SendMessage(CreateMessageDto createMessageDto) 
        {
            if (string.IsNullOrWhiteSpace(createMessageDto.Text) || createMessageDto.Text.Length > 1000)
            {
                {
                    _logger.LogWarning("SignalR: Отклонен спам-запрос (пустое или >1000 символов) от ConnectionId: {ConnectionId}", Context.ConnectionId);
                    return;
                }
            }

            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                {
                    _logger.LogError("SignalR: Критическая ошибка аутентификации! У авторизованного клиента отсутствует NameIdentifier.");
                    return;
                }
            }


            


            var messageDto = await _messageService.SaveMessageAsync(currentUserId, createMessageDto);

            if (messageDto == null) return;

            await Clients.All.SendAsync("ReceiveMessage", messageDto);


        }



    }
}
