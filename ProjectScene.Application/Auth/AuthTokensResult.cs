namespace ProjectScene.Application.Auth;

public class AuthTokensResult
{
    // JWT curto usado pelo frontend nas chamadas autenticadas.
    public string AccessToken { get; set; } = string.Empty;

    // Token longo usado apenas no fluxo de renovacao da sessao.
    public string RefreshToken { get; set; } = string.Empty;

    // Prazo absoluto do refresh token para escrita de cookie e validacao.
    public DateTime RefreshTokenExpiresAtUtc { get; set; }
}
