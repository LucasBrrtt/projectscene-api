using ProjectScene.Domain.Entities;

namespace ProjectScene.Domain.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user);

    Task<User?> GetByUsernameAsync(string username);

    bool VerifyPassword(User user, string password);
}
