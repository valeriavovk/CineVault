namespace CineVault.API.Controllers.Responses;

public sealed class UserStatsResponse
{
    public required int TotalReviews { get; set; }
    public required double AverageRating { get; set; }
    public required Dictionary<string, int> GenreStats { get; set; }
    public DateTime? LastActivity { get; set; }
}