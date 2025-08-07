namespace ProjectScene.API.DTOs.User;

public class CreateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string AccessLevel { get; set; } = "user";
}
