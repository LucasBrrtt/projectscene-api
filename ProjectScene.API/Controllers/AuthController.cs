using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectScene.API.DTOs.Auth;
using ProjectScene.Application.Interfaces;
using ProjectScene.Application.Options;
using Microsoft.Extensions.Options;

namespace ProjectScene.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly JwtOptions _jwtOptions;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        IAuthService authService,
        IOptions<JwtOptions> jwtOptions,
        IWebHostEnvironment environment)
    {
        _authService = authService;
        _jwtOptions = jwtOptions.Value;
        _environment = environment;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Tenta autenticar e gerar o token JWT para o usuário informado.
        var tokens = await _authService.LoginAsync(request.Username, request.Password, request.RememberMe);

        if (tokens == null)
        {
            return Unauthorized(new { message = "Usuario ou senha invalidos" });
        }

        WriteRefreshTokenCookie(tokens.RefreshToken, tokens.RefreshTokenExpiresAtUtc);

        return Ok(new AuthTokenResponse
        {
            AccessToken = tokens.AccessToken
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        // Usa o refresh token enviado por cookie para renovar a sessao sem expor o valor ao frontend.
        var refreshToken = Request.Cookies[_jwtOptions.RefreshTokenCookieName];
        var tokens = await _authService.RefreshAsync(refreshToken ?? string.Empty);

        if (tokens == null)
        {
            ClearRefreshTokenCookie();
            return Unauthorized(new { message = "Sessao expirada. Entre novamente." });
        }

        WriteRefreshTokenCookie(tokens.RefreshToken, tokens.RefreshTokenExpiresAtUtc);

        return Ok(new AuthTokenResponse
        {
            AccessToken = tokens.AccessToken
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // Invalida o refresh token atual e remove o cookie da resposta.
        var refreshToken = Request.Cookies[_jwtOptions.RefreshTokenCookieName];

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _authService.LogoutAsync(refreshToken);
        }

        ClearRefreshTokenCookie();
        return NoContent();
    }

    private void WriteRefreshTokenCookie(string refreshToken, DateTime expiresAtUtc)
    {
        // Mantem o refresh token restrito ao navegador via cookie HttpOnly.
        Response.Cookies.Append(
            _jwtOptions.RefreshTokenCookieName,
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Expires = expiresAtUtc,
                IsEssential = true,
                Path = "/"
            });
    }

    private void ClearRefreshTokenCookie()
    {
        // Expira o cookie para garantir que o navegador descarte a sessao renovavel.
        Response.Cookies.Delete(
            _jwtOptions.RefreshTokenCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Path = "/"
            });
    }
}
