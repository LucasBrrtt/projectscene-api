using ProjectScene.Domain.Entities;

namespace ProjectScene.Domain.Interfaces;

public interface IUserRepository
{
    // Define o acesso aos dados da entidade de usuário.
    Task AddAsync(User user);

    // Busca pelo identificador usado no login.
    Task<User?> GetByUsernameAsync(string username);

    // Localiza o usuario dono do refresh token atual.
    Task<User?> GetByRefreshTokenAsync(string refreshToken);

    // Permite validar duplicidade e fluxos baseados em email.
    Task<User?> GetByEmailAsync(string email);

    // Verifica se o username ja esta em uso.
    Task<bool> ExistsByUsernameAsync(string username);

    // Verifica se o email ja esta em uso.
    Task<bool> ExistsByEmailAsync(string email);

    // Persiste alteracoes em sessoes e outros dados mutaveis do usuario.
    Task UpdateAsync(User user);

    // Compara a senha informada com o hash armazenado.
    bool VerifyPassword(User user, string password);
}
