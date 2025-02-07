using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;
[Route("api")]
[ApiController]
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

    [HttpGet]
    public ActionResult ThrowException()
    {
        throw new NotImplementedException("This method should never be called");
    }
}
