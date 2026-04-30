using FluentAssertions;
using ProjectScene.Application.Options;

namespace ProjectScene.UnitTests.Infrastructure;

public class JwtOptionsTests
{
    [Fact]
    public void ResolveTokenExpirationMinutes_ShouldPreferExpirationMinutes()
    {
        var options = new JwtOptions
        {
            ExpirationMinutes = 15,
            ExpirationHours = 2
        };

        options.ResolveTokenExpirationMinutes().Should().Be(15);
    }

    [Fact]
    public void ResolveTokenExpirationMinutes_ShouldUseExpirationHours_WhenMinutesAreNotConfigured()
    {
        var options = new JwtOptions
        {
            ExpirationHours = 2
        };

        options.ResolveTokenExpirationMinutes().Should().Be(120);
    }

    [Fact]
    public void ResolveTokenExpirationMinutes_ShouldFallbackToSixtyMinutes()
    {
        var options = new JwtOptions();

        options.ResolveTokenExpirationMinutes().Should().Be(60);
    }

    [Fact]
    public void ResolveRefreshTokenExpiration_ShouldUseConfiguredDays_WhenRememberMeIsEnabled()
    {
        var options = new JwtOptions
        {
            RefreshTokenExpirationDays = 10
        };

        options.ResolveRefreshTokenExpiration(rememberMe: true).Should().Be(TimeSpan.FromDays(10));
    }

    [Fact]
    public void ResolveRefreshTokenExpiration_ShouldUseConfiguredHours_WhenRememberMeIsDisabled()
    {
        var options = new JwtOptions
        {
            RefreshTokenSessionExpirationHours = 8
        };

        options.ResolveRefreshTokenExpiration(rememberMe: false).Should().Be(TimeSpan.FromHours(8));
    }

    [Theory]
    [InlineData(true, 30, 0)]
    [InlineData(false, 0, 12)]
    public void ResolveRefreshTokenExpiration_ShouldUseFallbacks(bool rememberMe, int expectedDays, int expectedHours)
    {
        var options = new JwtOptions();
        var expected = expectedDays > 0 ? TimeSpan.FromDays(expectedDays) : TimeSpan.FromHours(expectedHours);

        options.ResolveRefreshTokenExpiration(rememberMe).Should().Be(expected);
    }
}
