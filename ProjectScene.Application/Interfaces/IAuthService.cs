namespace ProjectScene.Application.Interfaces;

public interface IAuthService
{
    // Autentica o usuário e retorna o JWT quando as credenciais são válidas.
    Task<string?> LoginAsync(string username, string password);
}
