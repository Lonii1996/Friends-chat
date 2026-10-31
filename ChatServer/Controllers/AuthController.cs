using ChatServer.DTOs;
using ChatServer.Entities;
using ChatServer.Interfaces;
using ChatServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ChatServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
           
                var result = await _authService.RegisterAsync(model);

                if (!result.IsSuccess)
                {
                    return BadRequest(new { message = result.Message });
                }
                return Ok(new { message = result.Message });
            
        }
        
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            
                var result = await _authService.LoginAsync(model);

                if (!result.IsSuccess)
                {
                    return Unauthorized(new { message = result.Message });
                }

                return Ok(result);
           

        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto model) 
        {
            var result = await _authService.RefreshTokenAsync(model);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }
    

    }
    
}
