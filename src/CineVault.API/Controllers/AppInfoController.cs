using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;

[Route("api")]
[ApiController]
public class AppInfoController : ControllerBase
{
    private readonly IWebHostEnvironment environment;
    private readonly ILogger<AppInfoController> logger;

    public AppInfoController(IWebHostEnvironment environment, ILogger<AppInfoController> logger)
    {
        this.environment = environment;
        this.logger = logger;
    }

    [HttpGet("environment")]
    public ActionResult<string> GetEnvironment()
    {
        var environment = new { this.environment.EnvironmentName };
        return this.Ok(environment);
    }

    [HttpGet("throw-exception")]
    public ActionResult ThrowException()
    {
        throw new NotImplementedException("This method should never be called");
    }

    [HttpGet("log-test")]
    public ActionResult LogTest()
    {
        this.logger.LogTrace("This is a Trace level message");
        this.logger.LogDebug("This is a Debug level message");
        this.logger.LogInformation("This is an Information level message");
        this.logger.LogWarning("This is a Warning level message");
        this.logger.LogError("This is an Error level message");
        this.logger.LogCritical("This is a Critical level message");

        return this.Ok("Log test completed.");
    }
}