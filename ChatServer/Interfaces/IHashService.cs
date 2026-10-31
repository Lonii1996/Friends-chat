namespace ChatServer.Interfaces
{
    public interface IHashService
    {
        string Hash(string refreshToken);
    }
}
