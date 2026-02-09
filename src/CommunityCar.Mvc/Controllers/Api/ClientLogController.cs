using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CommunityCar.Mvc.Controllers.Api;

[ApiController]
[Route("api/logs")]
public class ClientLogController : ControllerBase
{
    private readonly ILogger<ClientLogController> _logger;

    public ClientLogController(ILogger<ClientLogController> logger)
    {
        _logger = logger;
    }

    [HttpPost("client-error")]
    public IActionResult LogClientError([FromBody] ClientErrorModel error)
    {
        if (error == null)
        {
            return BadRequest("Error data is required");
        }

        // Log based on error type
        var logMessage = $"[CLIENT ERROR] {error.Type}: {error.Message} | URL: {error.Url} | User: {User.Identity?.Name ?? "Anonymous"}";

        switch (error.Type?.ToLower())
        {
            case "javascript error":
            case "unhandled promise rejection":
            case "ajax error":
            case "fetch error":
                _logger.LogError(logMessage + $" | Stack: {error.Stack}");
                break;
            
            case "console error":
                _logger.LogError(logMessage);
                break;
            
            case "console warn":
            case "resource load error":
                _logger.LogWarning(logMessage);
                break;
            
            default:
                _logger.LogInformation(logMessage);
                break;
        }

        // Optionally save to database for analysis
        // await _errorLogService.SaveClientErrorAsync(error);

        return Ok(new { success = true, message = "Error logged successfully" });
    }

    [HttpGet("client-errors")]
    public IActionResult GetClientErrors()
    {
        // This would retrieve errors from database
        // For now, return empty array
        return Ok(new { errors = new List<object>() });
    }
}

public class ClientErrorModel
{
    public string? Type { get; set; }
    public string? Message { get; set; }
    public string? Filename { get; set; }
    public int? Line { get; set; }
    public int? Column { get; set; }
    public string? Stack { get; set; }
    public string? Timestamp { get; set; }
    public string? Url { get; set; }
    public string? UserAgent { get; set; }
    public int? Status { get; set; }
    public string? StatusText { get; set; }
    public string? Response { get; set; }
    public string? Element { get; set; }
    public string? Id { get; set; }
    public string? ClassName { get; set; }
    public string? Text { get; set; }
    public string? Href { get; set; }
}
