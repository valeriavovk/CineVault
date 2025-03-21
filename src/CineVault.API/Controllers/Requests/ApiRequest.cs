namespace CineVault.API.Controllers.Requests;

public class ApiRequest
{
    public required string Username { get; set; }
    public required string SecretCode { get; set; }
    public required Dictionary<string, string> AdditionalProperties { get; set; }
    public required string NameOfServer { get; set; }
    public Guid RequestId { get; } = Guid.NewGuid();
}

public class ApiRequest<T> : ApiRequest
{
    public required T Data { get; set; }
}