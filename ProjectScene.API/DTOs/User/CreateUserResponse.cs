namespace ProjectScene.API.DTOs.User;

public class CreateUserResponse
{
    // Contrato de saida retornado apos o cadastro.
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
}
