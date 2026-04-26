namespace ProjectScene.Application.Options;

public class JwtOptions
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; }
    public int ExpirationHours { get; set; }

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
}
