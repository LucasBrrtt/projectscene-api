using Microsoft.AspNetCore.Mvc;
using ProjectScene.API.DTOs.User;
using ProjectScene.Application.Interfaces;

namespace ProjectScene.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
    {
        var user = await _userService.RegisterAsync(
            request.FullName,
            request.Email,
            request.Username,
            request.Password);

        var response = new CreateUserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Username = user.Username
        };

        return StatusCode(StatusCodes.Status201Created, response);
    }
}
