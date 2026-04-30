using System.ComponentModel.DataAnnotations;

namespace ProjectScene.API.DTOs.Auth;

public class LoginRequest
{
    // Contrato de entrada usado na autenticacao.
    [Required(ErrorMessage = "Username e obrigatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username deve ter entre 3 e 100 caracteres.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha e obrigatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres.")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
