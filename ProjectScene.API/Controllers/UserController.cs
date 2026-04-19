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
        // Encaminha o cadastro para a camada de aplicação.
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

        // Retorna apenas os dados públicos do usuário recém-criado.
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
