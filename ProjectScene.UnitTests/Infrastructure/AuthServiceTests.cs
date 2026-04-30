using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using ProjectScene.Application.Options;
using ProjectScene.Domain.Entities;
using ProjectScene.Domain.Interfaces;
using ProjectScene.Infrastructure.Services;

namespace ProjectScene.UnitTests.Infrastructure;

public class AuthServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly JwtOptions _jwtOptions = new()
    {
        Key = "0123456789abcdef0123456789abcdef",
        Issuer = "ProjectScene.Tests",
        Audience = "ProjectScene.Client.Tests",
        ExpirationMinutes = 30,
        RefreshTokenExpirationDays = 30,
        RefreshTokenSessionExpirationHours = 12
    };

    [Theory]
    [InlineData("", "secret")]
    [InlineData("lucas", "")]
    [InlineData("   ", "secret")]
    [InlineData("lucas", "   ")]
    public async Task LoginAsync_ShouldReturnNull_WhenCredentialsAreBlank(string username, string password)
    {
        var service = CreateService();

        var result = await service.LoginAsync(username, password, rememberMe: false);

        result.Should().BeNull();
        await _userRepository.DidNotReceive().GetByUsernameAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task LoginAsync_ShouldNormalizeUsernameBeforeLookup()
    {
        var service = CreateService();

        await service.LoginAsync("  LUCAS  ", "secret", rememberMe: false);

        await _userRepository.Received(1).GetByUsernameAsync("lucas");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var service = CreateService();

        var result = await service.LoginAsync("lucas", "secret", rememberMe: false);

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserIsInactive()
    {
        var user = CreateUser(isActive: false);
        _userRepository.GetByUsernameAsync("lucas").Returns(user);
        var service = CreateService();

        var result = await service.LoginAsync("lucas", "secret", rememberMe: false);

        result.Should().BeNull();
        _userRepository.DidNotReceive().VerifyPassword(Arg.Any<User>(), Arg.Any<string>());
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsInvalid()
    {
        var user = CreateUser();
        _userRepository.GetByUsernameAsync("lucas").Returns(user);
        _userRepository.VerifyPassword(user, "wrong").Returns(false);
        var service = CreateService();

        var result = await service.LoginAsync("lucas", "wrong", rememberMe: false);

        result.Should().BeNull();
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Theory]
    [InlineData(false, 12)]
    [InlineData(true, 720)]
    public async Task LoginAsync_ShouldIssueTokensAndUpdateUser_WhenCredentialsAreValid(
        bool rememberMe,
        int expectedRefreshTokenHours)
    {
        var beforeLogin = DateTime.UtcNow;
        var user = CreateUser();
        _userRepository.GetByUsernameAsync("lucas").Returns(user);
        _userRepository.VerifyPassword(user, "secret").Returns(true);
        var service = CreateService();

        var result = await service.LoginAsync("lucas", "secret", rememberMe);

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().Be(user.RefreshToken);
        result.RefreshTokenExpiresAtUtc.Should().Be(user.RefreshTokenExpiry);
        result.RefreshTokenExpiresAtUtc.Should()
            .BeCloseTo(beforeLogin.AddHours(expectedRefreshTokenHours), TimeSpan.FromSeconds(10));
        user.RefreshTokenPersistent.Should().Be(rememberMe);
        user.LastLogin.Should().NotBeNull();
        user.LastLogin.Should().BeOnOrAfter(beforeLogin);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        token.Issuer.Should().Be(_jwtOptions.Issuer);
        token.Audiences.Should().Contain(_jwtOptions.Audience);
        token.Claims.Should().Contain(claim => claim.Type == ClaimTypes.NameIdentifier && claim.Value == "7");
        token.Claims.Should().Contain(claim => claim.Type == ClaimTypes.Name && claim.Value == "lucas");
        token.Claims.Should().Contain(claim => claim.Type == ClaimTypes.Email && claim.Value == "lucas@example.com");
        token.Claims.Should().Contain(claim => claim.Type == ClaimTypes.Role && claim.Value == "admin");

        await _userRepository.Received(1).UpdateAsync(user);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RefreshAsync_ShouldReturnNull_WhenRefreshTokenIsBlank(string refreshToken)
    {
        var service = CreateService();

        var result = await service.RefreshAsync(refreshToken);

        result.Should().BeNull();
        await _userRepository.DidNotReceive().GetByRefreshTokenAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task RefreshAsync_ShouldReturnNull_WhenTokenDoesNotBelongToUser()
    {
        var service = CreateService();

        var result = await service.RefreshAsync("missing-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshAsync_ShouldReturnNull_WhenUserIsInactive()
    {
        var user = CreateUser(isActive: false);
        user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(1);
        _userRepository.GetByRefreshTokenAsync("refresh-token").Returns(user);
        var service = CreateService();

        var result = await service.RefreshAsync("refresh-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshAsync_ShouldReturnNull_WhenRefreshTokenIsExpired()
    {
        var user = CreateUser();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddSeconds(-1);
        _userRepository.GetByRefreshTokenAsync("refresh-token").Returns(user);
        var service = CreateService();

        var result = await service.RefreshAsync("refresh-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshAsync_ShouldRotateRefreshToken_WhenRefreshTokenIsValid()
    {
        var user = CreateUser();
        user.RefreshToken = "old-token";
        user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(1);
        user.RefreshTokenPersistent = false;
        _userRepository.GetByRefreshTokenAsync("old-token").Returns(user);
        var service = CreateService();

        var result = await service.RefreshAsync("old-token");

        result.Should().NotBeNull();
        result!.RefreshToken.Should().NotBe("old-token");
        result.RefreshToken.Should().Be(user.RefreshToken);
        user.RefreshTokenPersistent.Should().BeFalse();
        await _userRepository.Received(1).UpdateAsync(user);
    }

    [Fact]
    public async Task RefreshAsync_ShouldKeepLegacyAbsoluteExpiration_WhenPersistenceMetadataIsMissing()
    {
        var expiresAt = DateTime.UtcNow.AddHours(3);
        var user = CreateUser();
        user.RefreshToken = "legacy-token";
        user.RefreshTokenExpiry = expiresAt;
        user.RefreshTokenPersistent = null;
        _userRepository.GetByRefreshTokenAsync("legacy-token").Returns(user);
        var service = CreateService();

        var result = await service.RefreshAsync("legacy-token");

        result.Should().NotBeNull();
        result!.RefreshTokenExpiresAtUtc.Should().Be(expiresAt);
        user.RefreshTokenExpiry.Should().Be(expiresAt);
        user.RefreshTokenPersistent.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task LogoutAsync_ShouldDoNothing_WhenRefreshTokenIsBlank(string refreshToken)
    {
        var service = CreateService();

        await service.LogoutAsync(refreshToken);

        await _userRepository.DidNotReceive().GetByRefreshTokenAsync(Arg.Any<string>());
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task LogoutAsync_ShouldClearRefreshToken_WhenUserExists()
    {
        var user = CreateUser();
        user.RefreshToken = "refresh-token";
        user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(1);
        user.RefreshTokenPersistent = true;
        _userRepository.GetByRefreshTokenAsync("refresh-token").Returns(user);
        var service = CreateService();

        await service.LogoutAsync("refresh-token");

        user.RefreshToken.Should().BeNull();
        user.RefreshTokenExpiry.Should().BeNull();
        user.RefreshTokenPersistent.Should().BeNull();
        await _userRepository.Received(1).UpdateAsync(user);
    }

    private AuthService CreateService()
    {
        return new AuthService(_userRepository, Options.Create(_jwtOptions));
    }

    private static User CreateUser(bool isActive = true)
    {
        return new User
        {
            Id = 7,
            FullName = "Lucas Barrotti",
            Email = "lucas@example.com",
            Username = "lucas",
            PasswordHash = "hash",
            AccessLevel = "admin",
            IsActive = isActive
        };
    }
}
