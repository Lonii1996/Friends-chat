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
        private ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            return new ApplicationDbContext(options);
        }





        [Fact]
        public async Task RegisterAsync_ShouldReturnSuccess_WhenUserDoesNotExist()
        {
          
            using var context = CreateInMemoryDbContext();
            var mockConfiguration = new Mock<IJwtProvider>(); 
            var authService = new AuthService(context, mockConfiguration.Object);

            var registerDto = new RegisterDto
            {
                Name = "NewUser",
                Password = "SecretPassword123"
            };

            var result = await authService.RegisterAsync(registerDto);

            
            Assert.True(result.IsSuccess);
            Assert.Equal("Вы успешно зарегестрировались", result.Message);

            
            var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Name == "NewUser");
            Assert.NotNull(userInDb);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnFalse_WhenUserAlreadyExists()
        {
            
            using var context = CreateInMemoryDbContext();

           
            context.Users.Add(new User { Name = "ExistingUser", PasswordHash = "some_hash" });
            await context.SaveChangesAsync();

            var mockConfiguration = new Mock<IJwtProvider>();
            var authService = new AuthService(context, mockConfiguration.Object);

            
            var registerDto = new RegisterDto
            {
                Name = "ExistingUser",
                Password = "Password123"
            };

            
            var result = await authService.RegisterAsync(registerDto);

            
            Assert.False(result.IsSuccess);
            Assert.Equal("Имя уже занято", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnSuccess_WithToken_WhenCredentialsAreValid()
        {
            
            using var context = CreateInMemoryDbContext();

            
            string rawPassword = "CorrectPassword123";
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

           
            context.Users.Add(new User { Name = "User123", PasswordHash = passwordHash });
            await context.SaveChangesAsync();

            
            var mockConfiguration = new Mock<IJwtProvider>();
           

            var authService = new AuthService(context, mockConfiguration.Object);

            var loginDto = new LoginDto
            {
                Name = "User123",
                Password = rawPassword
            };

            
            var result = await authService.LoginAsync(loginDto);

            
            Assert.True(result.IsSuccess);
            
            Assert.False(string.IsNullOrEmpty(result.Message));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnFalse_WhenPasswordIsIncorrect()
        {
            
            using var context = CreateInMemoryDbContext();

            string passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123");
            context.Users.Add(new User { Name = "User123", PasswordHash = passwordHash });
            await context.SaveChangesAsync();

            var mockConfiguration = new Mock<IJwtProvider>();
            var authService = new AuthService(context, mockConfiguration.Object);

            var loginDto = new LoginDto
            {
                Name = "User123",
                Password = "WrongPassword!!!" 
            };

          
            var result = await authService.LoginAsync(loginDto);

        
            Assert.False(result.IsSuccess);
            Assert.Equal("Не верный пароль", result.Message);

        }

        [Fact]
        public async Task LoginAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            
            using var context = CreateInMemoryDbContext();
            

            var mockConfiguration = new Mock<IJwtProvider>();
            var authService = new AuthService(context, mockConfiguration.Object);

            var loginDto = new LoginDto
            {
                Name = "GhostUser", 
                Password = "AnyPassword123"
            };

            
            var result = await authService.LoginAsync(loginDto);

            
            Assert.False(result.IsSuccess);
            Assert.Equal("Пользователь не найден", result.Message); 
        }



    }
}

