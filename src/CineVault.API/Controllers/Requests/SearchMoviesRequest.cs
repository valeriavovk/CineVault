namespace CineVault.API.Controllers.Requests;

public sealed class SearchMoviesRequest
{
    public string? Genre { get; init; }
    public string? Title { get; init; }
    public string? Director { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public int? AvgRating { get; init; }
}