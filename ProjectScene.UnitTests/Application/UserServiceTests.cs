using FluentAssertions;
using NSubstitute;
using ProjectScene.Application.Exceptions;
using ProjectScene.Application.Interfaces;
using ProjectScene.Application.Services;
using ProjectScene.Domain.Entities;
using ProjectScene.Domain.Interfaces;

namespace ProjectScene.UnitTests.Application;

public class UserServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasherService _passwordHasherService = Substitute.For<IPasswordHasherService>();

    [Fact]
    public async Task RegisterAsync_ShouldNormalizeUserDataHashPasswordAndPersistUser()
    {
        User? savedUser = null;
        _passwordHasherService.HashPassword(Arg.Any<User>(), "secret").Returns("hashed-password");
        _userRepository.AddAsync(Arg.Do<User>(user => savedUser = user)).Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.RegisterAsync(
            "  Lucas Barrotti  ",
            "  LUCAS@EXAMPLE.COM  ",
            "  LucasUser  ",
            "secret");

        result.Should().BeSameAs(savedUser);
        savedUser.Should().NotBeNull();
        savedUser!.FullName.Should().Be("Lucas Barrotti");
        savedUser.Email.Should().Be("lucas@example.com");
        savedUser.Username.Should().Be("lucasuser");
        savedUser.AccessLevel.Should().Be("user");
        savedUser.IsActive.Should().BeTrue();
        savedUser.PasswordHash.Should().Be("hashed-password");
        savedUser.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        savedUser.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);

        await _userRepository.Received(1).ExistsByEmailAsync("lucas@example.com");
        await _userRepository.Received(1).ExistsByUsernameAsync("lucasuser");
        _passwordHasherService.Received(1).HashPassword(savedUser, "secret");
        await _userRepository.Received(1).AddAsync(savedUser);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowDuplicateResourceException_WhenEmailAlreadyExists()
    {
        _userRepository.ExistsByEmailAsync("lucas@example.com").Returns(true);
        var service = CreateService();

        var act = () => service.RegisterAsync("Lucas", "  LUCAS@EXAMPLE.COM  ", "lucas", "secret");

        await act.Should()
            .ThrowAsync<DuplicateResourceException>()
            .WithMessage("Ja existe um usuario com este email.");

        await _userRepository.DidNotReceive().ExistsByUsernameAsync(Arg.Any<string>());
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowDuplicateResourceException_WhenUsernameAlreadyExists()
    {
        _userRepository.ExistsByUsernameAsync("lucas").Returns(true);
        var service = CreateService();

        var act = () => service.RegisterAsync("Lucas", "lucas@example.com", "  LUCAS  ", "secret");

        await act.Should()
            .ThrowAsync<DuplicateResourceException>()
            .WithMessage("Ja existe um usuario com este username.");

        await _userRepository.Received(1).ExistsByEmailAsync("lucas@example.com");
        await _userRepository.Received(1).ExistsByUsernameAsync("lucas");
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }

    private UserService CreateService()
    {
        return new UserService(_userRepository, _passwordHasherService);
    }
}
