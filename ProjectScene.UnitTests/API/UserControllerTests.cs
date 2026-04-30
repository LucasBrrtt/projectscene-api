using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ProjectScene.API.Controllers;
using ProjectScene.API.DTOs.User;
using ProjectScene.Application.Interfaces;
using ProjectScene.Domain.Entities;

namespace ProjectScene.UnitTests.API;

public class UserControllerTests
{
    private readonly IUserService _userService = Substitute.For<IUserService>();

    [Fact]
    public async Task Register_ShouldReturnCreatedResponseWithoutPasswordHash()
    {
        _userService.RegisterAsync("Lucas", "lucas@example.com", "lucas", "secret").Returns(new User
        {
            Id = 10,
            FullName = "Lucas",
            Email = "lucas@example.com",
            Username = "lucas",
            PasswordHash = "hash"
        });
        var controller = new UserController(_userService);

        var result = await controller.Register(new CreateUserRequest
        {
            FullName = "Lucas",
            Email = "lucas@example.com",
            Username = "lucas",
            Password = "secret"
        });

        var created = result.Should().BeOfType<ObjectResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        created.Value.Should().BeEquivalentTo(new CreateUserResponse
        {
            Id = 10,
            FullName = "Lucas",
            Email = "lucas@example.com",
            Username = "lucas"
        });
        created.Value.Should().NotBeAssignableTo<User>();

        await _userService.Received(1).RegisterAsync("Lucas", "lucas@example.com", "lucas", "secret");
    }
}
