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

        // Registrado em UTC para não depender do fuso de quem criou o usuário.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Atualizado quando o login é concluído com sucesso.
        public DateTime? LastLogin { get; set; }

        // Token persistido para renovar a sessão sem reenviar credenciais.
        public string? RefreshToken { get; set; }

        // Define até quando o refresh token acima pode ser aceito.
        public DateTime? RefreshTokenExpiry { get; set; }

        // Indica se a sessao foi criada como persistente ("lembrar de mim").
        public bool? RefreshTokenPersistent { get; set; }

        // Identificador usado no login.
        public required string Username { get; set; }
    }
}
