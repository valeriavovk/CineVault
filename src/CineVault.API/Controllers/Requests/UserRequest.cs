namespace CineVault.API.Controllers.Requests;

public sealed class UserRequest
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}