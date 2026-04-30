using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using ProjectScene.API.Controllers;
using ProjectScene.API.DTOs.Auth;
using ProjectScene.Application.Auth;
using ProjectScene.Application.Interfaces;
using ProjectScene.Application.Options;

namespace ProjectScene.UnitTests.API;

public class AuthControllerTests
{
    private readonly IAuthService _authService = Substitute.For<IAuthService>();
    private readonly JwtOptions _jwtOptions = new()
    {
        RefreshTokenCookieName = "projectscene_refresh_token"
    };
    private readonly IWebHostEnvironment _environment = Substitute.For<IWebHostEnvironment>();

    public AuthControllerTests()
    {
        _environment.EnvironmentName.Returns(Environments.Development);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        var controller = CreateController();

        var result = await controller.Login(new LoginRequest
        {
            Username = "lucas",
            Password = "wrong"
        });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_ShouldReturnAccessTokenAndWriteRefreshCookie_WhenCredentialsAreValid()
    {
        _authService.LoginAsync("lucas", "secret", true).Returns(new AuthTokensResult
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            RefreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(30)
        });
        var controller = CreateController();

        var result = await controller.Login(new LoginRequest
        {
            Username = "lucas",
            Password = "secret",
            RememberMe = true
        });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new AuthTokenResponse
        {
            AccessToken = "access-token"
        });

        GetSetCookieHeader(controller).Should().Contain("projectscene_refresh_token=refresh-token");
        GetSetCookieHeader(controller).Should().Contain("httponly", Exactly.Once());
        GetSetCookieHeader(controller).Should().Contain("samesite=lax");
        GetSetCookieHeader(controller).Should().NotContain("secure");
    }

    [Fact]
    public async Task Refresh_ShouldClearCookieAndReturnUnauthorized_WhenRefreshTokenIsInvalid()
    {
        var controller = CreateController("projectscene_refresh_token=expired-token");

        var result = await controller.Refresh();

        result.Should().BeOfType<UnauthorizedObjectResult>();
        GetSetCookieHeader(controller).Should().Contain("projectscene_refresh_token=");
        GetSetCookieHeader(controller).Should().Contain("expires=");
    }

    [Fact]
    public async Task Refresh_ShouldReturnAccessTokenAndRotateCookie_WhenRefreshTokenIsValid()
    {
        _authService.RefreshAsync("old-refresh-token").Returns(new AuthTokensResult
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            RefreshTokenExpiresAtUtc = DateTime.UtcNow.AddHours(12)
        });
        var controller = CreateController("projectscene_refresh_token=old-refresh-token");

        var result = await controller.Refresh();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new AuthTokenResponse
        {
            AccessToken = "new-access-token"
        });
        GetSetCookieHeader(controller).Should().Contain("projectscene_refresh_token=new-refresh-token");
    }

    [Fact]
    public async Task Logout_ShouldReturnNoContentAndClearCookie_WhenCookieIsMissing()
    {
        var controller = CreateController();

        var result = await controller.Logout();

        result.Should().BeOfType<NoContentResult>();
        await _authService.DidNotReceive().LogoutAsync(Arg.Any<string>());
        GetSetCookieHeader(controller).Should().Contain("projectscene_refresh_token=");
    }

    [Fact]
    public async Task Logout_ShouldCallServiceReturnNoContentAndClearCookie_WhenCookieExists()
    {
        var controller = CreateController("projectscene_refresh_token=refresh-token");

        var result = await controller.Logout();

        result.Should().BeOfType<NoContentResult>();
        await _authService.Received(1).LogoutAsync("refresh-token");
        GetSetCookieHeader(controller).Should().Contain("projectscene_refresh_token=");
    }

    private AuthController CreateController(string? cookieHeader = null)
    {
        var httpContext = new DefaultHttpContext();

        if (!string.IsNullOrWhiteSpace(cookieHeader))
        {
            httpContext.Request.Headers.Cookie = cookieHeader;
        }

        return new AuthController(_authService, Options.Create(_jwtOptions), _environment)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    private static string GetSetCookieHeader(ControllerBase controller)
    {
        return controller.Response.Headers.SetCookie.ToString().ToLowerInvariant();
    }
}
