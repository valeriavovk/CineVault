namespace CineVault.API.Controllers.Responses;

public sealed class ActorResponse
{
    public required int Id { get; set; }
    public required string FullName { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Biography { get; set; }
}