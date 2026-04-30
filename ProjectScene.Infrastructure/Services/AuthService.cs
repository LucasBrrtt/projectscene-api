using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjectScene.Application.Auth;
using ProjectScene.Application.Interfaces;
using ProjectScene.Application.Options;
using ProjectScene.Domain.Entities;
using ProjectScene.Domain.Interfaces;

namespace ProjectScene.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtOptions _jwtOptions;

    public AuthService(IUserRepository userRepository, IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthTokensResult?> LoginAsync(string username, string password, bool rememberMe)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        // O login procura o usuario no mesmo formato salvo no cadastro.
        var normalizedUsername = username.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByUsernameAsync(normalizedUsername);

        // Nao gera token se o usuario nao existir, estiver inativo ou errar a senha.
        if (user is null || !user.IsActive || !_userRepository.VerifyPassword(user, password))
        {
            return null;
        }

        return await IssueTokensAsync(user, rememberMe);
    }

    public async Task<AuthTokensResult?> RefreshAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        // Reaproveita a sessao apenas quando o refresh token ainda for valido.
        var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);

        if (user is null || !user.IsActive || user.RefreshTokenExpiry is null || user.RefreshTokenExpiry <= DateTime.UtcNow)
        {
            return null;
        }

        // Preserva o tipo da sessao original; para tokens legados sem esse metadado,
        // renova sem estender o prazo absoluto ja gravado.
        return user.RefreshTokenPersistent.HasValue
            ? await IssueTokensAsync(user, user.RefreshTokenPersistent.Value)
            : await IssueTokensAsync(user, rememberMe: false, refreshTokenExpiresAtUtc: user.RefreshTokenExpiry.Value);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        // Remove o refresh token persistido para invalidar a sessao atual.
        var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
        if (user is null)
        {
            return;
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        user.RefreshTokenPersistent = null;

        await _userRepository.UpdateAsync(user);
    }

    private string GenerateJwtToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_jwtOptions.Key);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        // Os claims sao os dados que vao dentro do token e depois alimentam a autorizacao.
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.AccessLevel)
        };

        // Monta o JWT com emissor, audiencia, prazo e assinatura.
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ResolveTokenExpirationMinutes()),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<AuthTokensResult> IssueTokensAsync(User user, bool rememberMe, DateTime? refreshTokenExpiresAtUtc = null)
    {
        // Gera um novo refresh token a cada login e renovacao para evitar reutilizacao.
        var refreshToken = GenerateRefreshToken();
        var resolvedRefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc
            ?? DateTime.UtcNow.Add(_jwtOptions.ResolveRefreshTokenExpiration(rememberMe));

        user.LastLogin = DateTime.UtcNow;
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = resolvedRefreshTokenExpiresAtUtc;
        user.RefreshTokenPersistent = refreshTokenExpiresAtUtc.HasValue ? null : rememberMe;

        await _userRepository.UpdateAsync(user);

        return new AuthTokensResult
        {
            AccessToken = GenerateJwtToken(user),
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = resolvedRefreshTokenExpiresAtUtc
        };
    }

    private static string GenerateRefreshToken()
    {
        // Usa bytes criptograficamente seguros antes de serializar para texto.
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
