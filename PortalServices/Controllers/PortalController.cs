using Microsoft.AspNetCore.Mvc;
using PortalContracts;

namespace PortalServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortalController : ControllerBase
{
    private readonly IPortalEngine _engine;
    private readonly ILogger<PortalController> _logger;

    public PortalController(IPortalEngine engine, ILogger<PortalController> logger)
    {
        _engine = engine;
        _logger = logger;
    }

    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new { Status = "OK", Timestamp = DateTime.UtcNow });
    }

    [HttpPost("process")]
    public IActionResult Process([FromBody] PortalRequest request)
    {
        _logger.LogInformation("Processing portal request for {CustomerId} in action {Action}", request.CustomerId, request.Action);
        var response = _engine.Process(request);
        return Ok(response);
    }
}
