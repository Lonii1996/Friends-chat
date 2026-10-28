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

        public ChatHub(IMessageService messageService) 
        {
            _messageService = messageService;
        }
        
        public async Task SendMessage(CreateMessageDto createMessageDto) 
        {
            if (string.IsNullOrWhiteSpace(createMessageDto.Text) || createMessageDto.Text.Length > 1000) return;

            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId)) return;


            


            var messageDto = await _messageService.SaveMessageAsync(currentUserId, createMessageDto);

            if (messageDto == null) return;

            await Clients.All.SendAsync("ReceiveMessage", messageDto);


        }



    }
}
