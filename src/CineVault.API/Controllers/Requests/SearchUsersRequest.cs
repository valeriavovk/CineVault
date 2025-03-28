namespace CineVault.API.Controllers.Requests;

public sealed class SearchUsersRequest
{
    public string? SearchTerm { get; init; }
    public DateTime? CreatedAfter { get; init; }
    public DateTime? CreatedBefore { get; init; }
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; }
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
}