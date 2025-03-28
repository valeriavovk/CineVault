namespace CineVault.API.Controllers.Requests;

public sealed class LikeRequest
{
    public required int ReviewId { get; init; }
    public required int UserId { get; init; }
}