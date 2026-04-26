using ProjectScene.Domain.Entities;

namespace ProjectScene.Application.Interfaces;

public interface IUserService
{
    // Executa o cadastro público de um novo usuário.
    Task<User> RegisterAsync(string fullName, string email, string username, string password);
}
