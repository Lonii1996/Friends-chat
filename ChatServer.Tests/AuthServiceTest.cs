using ChatServer.Context;
using ChatServer.DTOs;
using ChatServer.Entities;
using ChatServer.Interfaces;
using ChatServer.Options;
using ChatServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Cryptography;
using System.Text;
using Xunit;


namespace ChatServer.Tests
{
    public class AuthServiceTest
    {

        private readonly ApplicationDbContext _context;
        private readonly Mock<IJwtProvider> _mockJwt;
        
        private readonly AuthService _authService;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly IHashService _hashService;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;


        public AuthServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            
            _context = new ApplicationDbContext(options);
            _mockJwt = new Mock<IJwtProvider>();
            
            _mockLogger = new Mock<ILogger<AuthService>>();
            _hashService = new HashService();
            _refreshTokenGenerator = new RefreshTokenGenerator();

            

            var jwtOptions = new JwtOptions
            {
                AccessTokenExpirationMinutes = 15,
                RefreshTokenExpirationDays = 30
            };
            var mockOptions = Microsoft.Extensions.Options.Options.Create(jwtOptions);

            _authService = new AuthService(_context, _mockJwt.Object, _hashService, _refreshTokenGenerator, mockOptions, _mockLogger.Object);

           
            
        }


       






        [Fact]
        public async Task RegisterAsync_ShouldReturnSuccess_WhenUserDoesNotExist()
        {
          
            
            var registerDto = new RegisterDto
            {
                Name = "NewUser",
                Password = "SecretPassword123"
            };

            var result = await _authService.RegisterAsync(registerDto);

            
            Assert.True(result.IsSuccess);
            Assert.Equal("Вы успешно зарегестрировались", result.Message);

            
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Name == "NewUser");
            Assert.NotNull(userInDb);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnFalse_WhenUserAlreadyExists()
        {

            var User = new User { Name = "ExistingUser", PasswordHash = "123" };

            _context.Users.Add(User);
            await _context.SaveChangesAsync();



            var registerDto = new RegisterDto
            {
                Name = "ExistingUser",
                Password = "Password123"
            };

            
            var result = await _authService.RegisterAsync(registerDto);

            
            Assert.False(result.IsSuccess);
            Assert.Equal("Имя уже занято", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnSuccess_WithToken_WhenCredentialsAreValid()
        {
            string rawPassword = "12345";
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);
            
            _context.Users.Add(new User { Name = "User123", PasswordHash = passwordHash });
            await _context.SaveChangesAsync();


            

            var loginDto = new LoginDto
            {
                Name = "User123",
                Password = rawPassword
            };

            
            var result = await _authService.LoginAsync(loginDto);

            
            Assert.True(result.IsSuccess);
            
            Assert.False(string.IsNullOrEmpty(result.Message));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnFalse_WhenPasswordIsIncorrect()
        {
            
            

            string passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123");
            _context.Users.Add(new User { Name = "User123", PasswordHash = passwordHash });
            await _context.SaveChangesAsync();

         
            var loginDto = new LoginDto
            {
                Name = "User123",
                Password = "WrongPassword!!!" 
            };

          
            var result = await _authService.LoginAsync(loginDto);

        
            Assert.False(result.IsSuccess);
            Assert.Equal("Не верный пароль", result.Message);

        }

        [Fact]
        public async Task LoginAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            var loginDto = new LoginDto
            {
                Name = "GhostUser", 
                Password = "AnyPassword123"
            };

            
            var result = await _authService.LoginAsync(loginDto);

            
            Assert.False(result.IsSuccess);
            Assert.Equal("Пользователь не найден", result.Message); 
        }

        


        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnSuccess_AndRotateTokens_WhenTokenIsValid()
        {
            var refreshToken = _refreshTokenGenerator.GenerateRefreshToken();
            var refreshTokenHash = _hashService.Hash(refreshToken);

            var testUser = new User
            {
                Name = "TestName",
                PasswordHash = "TestPasswordHash"
            };

            var testUserRefreshToken = new UserRefreshToken
            {
                TokenHash = refreshTokenHash,
                User = testUser,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            
            _context.Add(testUserRefreshToken);
            await _context.SaveChangesAsync();

            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = refreshToken
            };

            
            var result = await _authService.RefreshTokenAsync(refreshTokenDto);

            var newRefreshTokenHash = _hashService.Hash(result.RefreshToken!);

            var newTokenHashInDb = await _context.UsersRefreshToken.AsNoTracking().FirstOrDefaultAsync(r => r.TokenHash == newRefreshTokenHash);

            var invalidRefreshTokenHash = await _context.UsersRefreshToken.AsNoTracking().FirstOrDefaultAsync(r => r.TokenHash == refreshTokenHash);


            Assert.True(result.IsSuccess);
            Assert.Null(invalidRefreshTokenHash);
            Assert.NotNull(newTokenHashInDb);

        }






        [Fact]
        public async Task RefreshTokenAsync_ShouldRejectReusedToken_AfterRotation()
        {
            var refreshToken = _refreshTokenGenerator.GenerateRefreshToken();
            var refreshTokenHash = _hashService.Hash(refreshToken);

            var testUser = new User
            {
                Name = "TestName",
                PasswordHash = "TestPasswordHash"
            };

            var testUserRefreshToken = new UserRefreshToken
            {
                TokenHash = refreshTokenHash,
                User = testUser,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            
            _context.Add(testUserRefreshToken);
            await _context.SaveChangesAsync();

          

            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = refreshToken
            };


            await _authService.RefreshTokenAsync(refreshTokenDto);
            var result = await _authService.RefreshTokenAsync(refreshTokenDto);


            Assert.False(result.IsSuccess);
            

            

        }








    }
}

