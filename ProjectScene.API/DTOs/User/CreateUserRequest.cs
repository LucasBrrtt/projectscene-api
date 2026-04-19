using System.ComponentModel.DataAnnotations;

namespace ProjectScene.API.DTOs.User;

public class CreateUserRequest
{
    // Contrato de entrada usado no cadastro publico.
    [Required(ErrorMessage = "Nome completo e obrigatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome completo deve ter entre 3 e 100 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email e obrigatorio.")]
    [EmailAddress(ErrorMessage = "Email invalido.")]
    [StringLength(150, ErrorMessage = "Email deve ter no maximo 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha e obrigatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username e obrigatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username deve ter entre 3 e 100 caracteres.")]
    public string Username { get; set; } = string.Empty;
}
