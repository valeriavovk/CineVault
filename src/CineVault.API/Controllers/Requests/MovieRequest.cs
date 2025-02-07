namespace CineVault.API.Controllers.Requests;

public sealed class MovieRequest
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public string? Genre { get; init; }
    public string? Director { get; init; }
}