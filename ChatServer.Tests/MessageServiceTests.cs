using ChatServer.Context;
using ChatServer.DTOs;
using ChatServer.Entities;
using ChatServer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;


namespace ChatServer.Tests
{
    public class MessageServiceTests
    {
        private ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task SaveMessageAsync_ShouldReturnSuccess_WhenUserExists() 
        {
            using var context = CreateInMemoryDbContext();

            var userTest = new User { Name = "TestName" };

            var createMessageDtoTest = new CreateMessageDto { Text = "TestMessage" };
            
            context.Users.Add(userTest);
            await context.SaveChangesAsync();

            var messageService = new MessageService(context);

            var result = await messageService.SaveMessageAsync(userTest.Id, createMessageDtoTest);


            Assert.NotNull(result);
            Assert.Equal("TestMessage", result.Text);


        }
        



    }
}
