using Microsoft.AspNetCore.Identity;
using ProjectScene.Application.Interfaces;
using ProjectScene.Domain.Entities;

namespace ProjectScene.Infrastructure.Services;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(User user, string password)
    {
        // Usa o hasher padrão do ASP.NET para gerar o valor persistido.
        return _passwordHasher.HashPassword(user, password);
    }
}
