namespace CineVault.API.Controllers.Responses;

public class ApiResponse
{
    public required int StatusCode { get; set; }
    public required string Message { get; set; }
    public Guid ResponseId { get; } = Guid.NewGuid();
}

public class ApiResponse<T> : ApiResponse
{
    public required T Data { get; set; }
}