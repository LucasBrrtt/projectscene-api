using ProjectScene.Domain.Entities;

namespace ProjectScene.Application.Interfaces;

public interface IPasswordHasherService
{
    string HashPassword(User user, string password);
}
