namespace CineVault.API.Controllers.Requests;

public sealed class ReviewRequest
{
    public required int MovieId { get; init; }
    public required int UserId { get; init; }
    public required int Rating { get; init; }
    public string? Comment { get; init; }
}