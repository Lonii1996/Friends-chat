using ChatServer.Interfaces;
using ChatServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        
        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory([FromQuery] int? cursorMessageID, int count = 20)
        {
           if (count  > 50) count = 50;
                
           var history = await _messageService.GetHistoryAsync(cursorMessageID, count);

            return Ok(history);
            
        }

    }
}
