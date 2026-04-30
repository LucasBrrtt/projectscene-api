namespace ProjectScene.API.DTOs.Auth;

public class AuthTokenResponse
{
    // O frontend recebe apenas o access token; o refresh token fica no cookie HttpOnly.
    public string AccessToken { get; set; } = string.Empty;
}
