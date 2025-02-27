using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;
[Route("api/v{v:apiVersion}")]
[ApiController]
[ApiVersion(1)]
[ApiVersion(2)]
public class AppInfoController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public AppInfoController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("environment")]
    public ActionResult<string> GetEnvironment()
    {
        var environment = new { EnvironmentName = _environment.EnvironmentName };

        return Ok(environment);
    }

    [HttpGet("throw-exception")]
    public ActionResult ThrowException()
    {
        throw new NotImplementedException("This method should never be called");
    }

    [MapToApiVersion(1)]
    [HttpGet("old-endpoint")]
    public IActionResult OldVersionEndpoint()
    {
        return Ok("Old version endpoint");
    }

    [MapToApiVersion(2)]
    [HttpGet("new-endpoint")]
    public IActionResult NewVersionEndpoint()
    {
        return Ok("New version endpoint");
    }
}
