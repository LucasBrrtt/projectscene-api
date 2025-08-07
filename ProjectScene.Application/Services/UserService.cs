using ProjectScene.Application.Interfaces;
using ProjectScene.Domain.Entities;
using ProjectScene.Domain.Interfaces;

namespace ProjectScene.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> CreateAsync(User user)
    {
        await _userRepository.AddAsync(user);
        return user;
    }
}
