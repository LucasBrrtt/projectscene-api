using ProjectScene.Domain.Entities;

namespace ProjectScene.Application.Interfaces;

public interface IUserService
{
    Task<User> CreateAsync(User user);
}
