using ProjectScene.Domain.Entities;

namespace ProjectScene.Application.Interfaces;

public interface IUserService
{
    Task<User> RegisterAsync(string fullName, string email, string username, string password);
}
