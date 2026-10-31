using ChatServer.Context;
using ChatServer.DTOs;
using ChatServer.Entities;
using ChatServer.Interfaces;
using ChatServer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;


namespace ChatServer.Tests
{
    public class AuthServiceTest
    {

        private readonly ApplicationDbContext _context;
        private readonly Mock<IJwtProvider> _mockJwt;
        private readonly Mock<IHashService> _mockHash;
        private readonly Mock<IRefreshTokenGenerator> _mockRefresh;
        private readonly AuthService _authService;


        public AuthServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            
            _context = new ApplicationDbContext(options);
            _mockJwt = new Mock<IJwtProvider>();
            _mockHash = new Mock<IHashService>();
            _mockRefresh = new Mock<IRefreshTokenGenerator>();
            _authService = new AuthService(_context, _mockJwt.Object, _mockHash.Object, _mockRefresh.Object);

            _mockHash.Setup(h => h.Hash(It.IsAny<string>())).Returns("fake_hash_value");

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



    }
}

