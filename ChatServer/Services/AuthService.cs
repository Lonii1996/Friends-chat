using ChatServer.Context;
using ChatServer.DTOs;
using ChatServer.Entities;
using ChatServer.Interfaces;
using ChatServer.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;


namespace ChatServer.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtProvider _jwtProvider;
        private readonly IHashService _hashService;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly JwtOptions _jwtOptions;
        private readonly ILogger<IAuthService> _logger;
     

        

        public AuthService(ApplicationDbContext context, IJwtProvider jwtProvider, IHashService hashService, IRefreshTokenGenerator refreshTokenGenerator, IOptions<JwtOptions> jwtOptions, ILogger<IAuthService> logger)
        {
            _context = context;
            _jwtProvider = jwtProvider;
            _hashService = hashService;
            _refreshTokenGenerator = refreshTokenGenerator;
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto model)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Name == model.Name);
            if (userExists)
            {
                _logger.LogWarning("Регистрация отклонена: Имя {Name} уже занято.", model.Name);
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
                _logger.LogInformation("Успешно зарегистрирован новый пользователь: {Name}", model.Name);
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
                _logger.LogWarning("Попытка входа под несуществующим именем: {Name}", model.Name);
                return new AuthResult { IsSuccess = false, Message = "Пользователь не найден" };
            }

            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                _logger.LogWarning("Неудачная попытка входа для пользователя {Name}: неверный пароль.", model.Name);
                return new AuthResult { IsSuccess = false, Message = "Не верный пароль" };
            }



            var accessToken = _jwtProvider.GenerateAccessToken(user);
            var refreshToken = _refreshTokenGenerator.GenerateRefreshToken();
            var refreshTokenHash = _hashService.Hash(refreshToken);


            var userRefreshToken = new UserRefreshToken
            {
                TokenHash = refreshTokenHash,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
            };
            
            _context.UsersRefreshToken.Add(userRefreshToken);
            await _context.SaveChangesAsync();



            _logger.LogInformation("Пользователь {Name} (Id: {Id}) успешно авторизован в системе.", user.Name, user.Id);

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


        public async Task<AuthResult> RefreshTokenAsync(RefreshTokenDto refreshToken) 
        {
            var incomingHash = _hashService.Hash(refreshToken.RefreshToken);

            var savedRefreshTokenHash = await _context.UsersRefreshToken
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == incomingHash);

            if (savedRefreshTokenHash == null) 
            {
                _logger.LogError("Безопасность: Попытка обновления сессии с поддельным или несуществующим рефреш-токеном!");
                return new AuthResult { IsSuccess = false, Message = "Невалидный рефреш-токен" };
            }
            
            var user = savedRefreshTokenHash.User;

            if (savedRefreshTokenHash.IsExpired)
            {
                _logger.LogWarning("Сессия пользователя {UserId} протухла по времени действия рефреш-токена.", savedRefreshTokenHash.UserId);
                _context.UsersRefreshToken.Remove(savedRefreshTokenHash);
                await _context.SaveChangesAsync();

                return new AuthResult { IsSuccess = false, Message = "Срок действия сессии истек" };
                
            }

            _context.UsersRefreshToken.Remove(savedRefreshTokenHash);

            var newAccessToken = _jwtProvider.GenerateAccessToken(user!);
            var newRefreshToken = _refreshTokenGenerator.GenerateRefreshToken();
            var newRefreshTokenHash = _hashService.Hash(newRefreshToken);

            var userRefreshToken = new UserRefreshToken
            {
                TokenHash = newRefreshTokenHash,
                UserId = user!.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
            };

            _context.UsersRefreshToken.Add(userRefreshToken);
            await _context.SaveChangesAsync();

            return new AuthResult
            {
                IsSuccess = true,
                Message = "Сессия успешно обновлена",
                UserId = user.Id,
                Username = user.Name,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };
        }




        
    }
}
