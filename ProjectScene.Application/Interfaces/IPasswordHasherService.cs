using ProjectScene.Domain.Entities;

namespace ProjectScene.Application.Interfaces;

public interface IPasswordHasherService
{
    // Gera o hash da senha antes da persistência.
    string HashPassword(User user, string password);
}
