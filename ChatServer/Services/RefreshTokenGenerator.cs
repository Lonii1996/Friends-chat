using ChatServer.Interfaces;
using System.Security.Cryptography;

namespace ChatServer.Services
{
    public class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
