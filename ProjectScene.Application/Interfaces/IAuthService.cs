using ProjectScene.Application.Auth;

namespace ProjectScene.Application.Interfaces;

public interface IAuthService
{
    // Autentica o usuario, registra a sessao e devolve os tokens gerados.
    Task<AuthTokensResult?> LoginAsync(string username, string password, bool rememberMe);

    // Renova a sessao a partir de um refresh token previamente persistido.
    Task<AuthTokensResult?> RefreshAsync(string refreshToken);

    // Revoga o refresh token informado para encerrar a sessao atual.
    Task LogoutAsync(string refreshToken);
}
