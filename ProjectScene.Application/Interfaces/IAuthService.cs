namespace ProjectScene.Application.Interfaces;

public interface IAuthService
{
    string GenerateToken(string userId);
    Task<string?> LoginAsync(string username, string password);
}
