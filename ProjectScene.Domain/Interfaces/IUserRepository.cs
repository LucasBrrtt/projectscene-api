using ProjectScene.Domain.Entities;

namespace ProjectScene.Domain.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user);
}
