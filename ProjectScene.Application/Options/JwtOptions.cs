namespace ProjectScene.Application.Options
{
    public class JwtOptions
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; }
        public int ExpirationHours { get; set; }

        public int GetTokenExpirationMinutes()
        {
            if (ExpirationMinutes > 0)
            {
                return ExpirationMinutes;
            }

            if (ExpirationHours > 0)
            {
                return ExpirationHours * 60;
            }

            return 60;
        }
    }
}
