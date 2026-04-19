using ProjectScene.Domain.Entities;

namespace ProjectScene.Domain.Interfaces;

public interface IUserRepository
{
    // Define o acesso aos dados da entidade de usuário.
    Task AddAsync(User user);

    Task<User?> GetByUsernameAsync(string username);

    Task<User?> GetByEmailAsync(string email);

    Task<bool> ExistsByUsernameAsync(string username);

    Task<bool> ExistsByEmailAsync(string email);

    bool VerifyPassword(User user, string password);
}
