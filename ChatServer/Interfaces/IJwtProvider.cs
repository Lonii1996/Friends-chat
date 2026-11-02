using ChatServer.DTOs;
using ChatServer.Entities;


namespace ChatServer.Interfaces
{
    public interface IJwtProvider
    {

        public string GenerateAccessToken(User user);
        



    }
}
