namespace ProjectScene.Application.Options;

public class JwtOptions
{
    // Chave simetrica usada para assinar os access tokens.
    public string Key { get; set; } = string.Empty;

    // Emissor validado durante a autenticacao Bearer.
    public string Issuer { get; set; } = string.Empty;

    // Audiencia esperada pelos clientes que consomem a API.
    public string Audience { get; set; } = string.Empty;

    // Duracao principal do access token em minutos.
    public int ExpirationMinutes { get; set; }

    // Mantido por compatibilidade com configuracoes antigas.
    public int ExpirationHours { get; set; }

    // Prazo maior usado quando o usuario escolhe manter a sessao.
    public int RefreshTokenExpirationDays { get; set; }

    // Prazo menor usado para sessoes sem persistencia longa.
    public int RefreshTokenSessionExpirationHours { get; set; }

    // Nome do cookie HttpOnly que transporta o refresh token.
    public string RefreshTokenCookieName { get; set; } = "projectscene_refresh_token";

    public int ResolveTokenExpirationMinutes()
    {
        if (ExpirationMinutes > 0)
        {
            return ExpirationMinutes;
        }

        if (ExpirationHours > 0)
        {
            // Mantém compatibilidade com configuração antiga em horas.
            return ExpirationHours * 60;
        }

        // Fallback padrão quando nada foi configurado.
        return 60;
    }

    public TimeSpan ResolveRefreshTokenExpiration(bool rememberMe)
    {
        // O tempo do refresh token muda conforme a preferencia de persistencia da sessao.
        if (rememberMe)
        {
            if (RefreshTokenExpirationDays > 0)
            {
                return TimeSpan.FromDays(RefreshTokenExpirationDays);
            }

            return TimeSpan.FromDays(30);
        }

        if (RefreshTokenSessionExpirationHours > 0)
        {
            return TimeSpan.FromHours(RefreshTokenSessionExpirationHours);
        }

        return TimeSpan.FromHours(12);
    }
}
