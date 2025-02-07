namespace CineVault.API.Controllers.Responses;

public sealed class UserResponse
{
    public required int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
}