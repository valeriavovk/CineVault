namespace CineVault.API.Controllers.Requests;

public sealed class ActorRequest
{
    public required string FullName { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Biography { get; set; }
}