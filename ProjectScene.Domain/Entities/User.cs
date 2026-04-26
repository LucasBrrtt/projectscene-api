namespace ProjectScene.Domain.Entities
{
    public class User
    {
        // Representa o usuario persistido e autenticado pela API.
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        // Armazena apenas o hash da senha, nunca a senha em texto puro.
        public string PasswordHash { get; set; } = string.Empty;

        // Define o perfil usado nas regras de autorizacao.
        public string AccessLevel { get; set; } = "user";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLogin { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiry { get; set; }

        // Identificador usado no login.
        public required string Username { get; set; }
    }
}
