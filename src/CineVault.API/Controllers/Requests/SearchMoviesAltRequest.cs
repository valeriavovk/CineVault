namespace CineVault.API.Controllers.Requests;

public sealed class SearchMoviesAltRequest
{
    public string? Text { get; init; }
    public string? Genre { get; init; }
    public int? MinRating { get; init; }
    public DateOnly? ReleaseDate { get; init; }
}