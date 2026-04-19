using ProjectScene.Application.Exceptions;
using ProjectScene.Application.Interfaces;
using ProjectScene.Domain.Entities;
using ProjectScene.Domain.Interfaces;

namespace ProjectScene.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;

    public UserService(IUserRepository userRepository, IPasswordHasherService passwordHasherService)
    {
        _userRepository = userRepository;
        _passwordHasherService = passwordHasherService;
    }

    public async Task<User> RegisterAsync(string fullName, string email, string username, string password)
    {
        // Padroniza os dados para evitar salvar variacoes desnecessarias.
        var normalizedFullName = fullName.Trim();
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedUsername = username.Trim().ToLowerInvariant();

        // Bloqueia cadastro com email ja usado.
        if (await _userRepository.ExistsByEmailAsync(normalizedEmail))
        {
            throw new DuplicateResourceException("Ja existe um usuario com este email.");
        }

        // Bloqueia cadastro com username ja usado.
        if (await _userRepository.ExistsByUsernameAsync(normalizedUsername))
        {
            throw new DuplicateResourceException("Ja existe um usuario com este username.");
        }

        // Define os dados internos controlados pela aplicacao.
        var user = new User
        {
            FullName = normalizedFullName,
            Email = normalizedEmail,
            Username = normalizedUsername,
            AccessLevel = "user",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Converte a senha recebida em hash antes de salvar no banco.
        user.PasswordHash = _passwordHasherService.HashPassword(user, password);

        await _userRepository.AddAsync(user);
        return user;
    }
}
