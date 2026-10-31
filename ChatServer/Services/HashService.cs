
using System;
using System.Security.Cryptography;
using System.Text;
using ChatServer.Interfaces;

namespace ChatServer.Services
{
    public class HashService : IHashService
    {
        public string Hash(string refreshToken)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(refreshToken);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
