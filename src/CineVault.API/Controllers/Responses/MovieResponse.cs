namespace CineVault.API.Controllers.Responses;

public sealed class MovieResponse
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? Genre { get; set; }
    public string? Director { get; set; }
    public required double AverageRating { get; set; }
    public required int ReviewCount { get; set; }
}