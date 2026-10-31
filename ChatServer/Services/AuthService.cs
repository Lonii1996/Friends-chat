using ChatServer.Context;
using ChatServer.DTOs;
using ChatServer.Entities;
using ChatServer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;


namespace ChatServer.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtProvider _jwtProvider;
        private readonly IHashService _hashService;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
     

        

        public AuthService(ApplicationDbContext context, IJwtProvider jwtProvider, IHashService hashService, IRefreshTokenGenerator refreshTokenGenerator)
        {
            _context = context;
            _jwtProvider = jwtProvider;
            _hashService = hashService;
            _refreshTokenGenerator = refreshTokenGenerator;
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto model)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Name == model.Name);
            if (userExists)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    Message = "Имя уже занято"
                };
            }

            var newUser = new User
            {
                Name = model.Name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            try
            {
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                return new AuthResult { IsSuccess = true, Message = "Вы успешно зарегестрировались" };
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    Message = "Имя уже занято"

                };
            }
        }

        




        public async Task<AuthResult> LoginAsync(LoginDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == model.Name);

            if ( user == null)
            {
                return new AuthResult { IsSuccess = false, Message = "Пользователь не найден" };
            }

            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return new AuthResult { IsSuccess = false, Message = "Не верный пароль" };
            }



            var accessToken = _jwtProvider.GenerateAccessToken(user);
            var refreshToken = _refreshTokenGenerator.GenerateRefreshToken();
            var refreshTokenHash = _hashService.Hash(refreshToken);


            var userRefreshToken = new UserRefreshToken
            {
                TokenHash = refreshTokenHash,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };
            
            _context.UsersRefreshToken.Add(userRefreshToken);
            await _context.SaveChangesAsync();




            return new AuthResult
            {
                IsSuccess = true,
                Message = "Вход выполнен успешно!",
                UserId = user.Id,
                Username = user.Name,
                AccessToken = accessToken,
                RefreshToken = refreshToken,


            };
        }




        
    }
}
