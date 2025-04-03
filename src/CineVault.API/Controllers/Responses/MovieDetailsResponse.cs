namespace CineVault.API.Controllers.Responses;

public sealed class MovieDetailsResponse
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? Genre { get; set; }
    public string? Director { get; set; }
    public required double AverageRating { get; set; }
    public required int ReviewCount { get; set; }
    public required List<ReviewUserResponse> LastReviews { get; set; }
    
    public sealed class ReviewUserResponse
    {
        public required int ReviewId { get; set; }
        public required int Rating { get; set; }
        public string? Comment { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required UserResponse User { get; set; }
    }
}