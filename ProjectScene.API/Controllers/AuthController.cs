using Microsoft.AspNetCore.Mvc;
using ProjectScene.API.DTOs.Auth;
using ProjectScene.Application.Interfaces;

namespace ProjectScene.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request.Username, request.Password);

        if (token == null)
        {
            return Unauthorized(new { message = "Usuario ou senha invalidos" });
        }

        return Ok(new { token });
    }
}
