using Microsoft.AspNetCore.Mvc;
using ProjectScene.API.DTOs.User;
using ProjectScene.Application.Interfaces;
using ProjectScene.Domain.Entities;

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
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = request.PasswordHash,
            AccessLevel = request.AccessLevel,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userService.CreateAsync(user);

        return Ok(new { message = "Usuário criado com sucesso!" });
    }
}
