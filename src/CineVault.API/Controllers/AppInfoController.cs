using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;
[Route("api")]
[ApiController]
public class AppInfoController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AppInfoController> _logger;

    public AppInfoController(IWebHostEnvironment environment, ILogger<AppInfoController> logger)
    {
        _environment = environment;
        _logger = logger;
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

    [HttpGet("log-test")]
    public ActionResult LogTest()
    {
        _logger.LogTrace("This is a Trace level message");
        _logger.LogDebug("This is a Debug level message");
        _logger.LogInformation("This is an Information level message");
        _logger.LogWarning("This is a Warning level message");
        _logger.LogError("This is an Error level message");
        _logger.LogCritical("This is a Critical level message");

        return Ok("Log test completed.");
    }
}