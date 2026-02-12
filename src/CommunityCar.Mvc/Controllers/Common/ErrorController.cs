using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using CommunityCar.Mvc.Controllers.Base;

namespace CommunityCar.Mvc.Controllers.Common;

public class ErrorController : BaseController
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Route("Error/{statusCode}")]
    public IActionResult Index(int statusCode)
    {
        var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        
        _logger.LogWarning(
            "Error {StatusCode} occurred. Path: {Path}, QueryString: {QueryString}",
            statusCode,
            statusCodeResult?.OriginalPath,
            statusCodeResult?.OriginalQueryString);

        ViewBag.StatusCode = statusCode;
        ViewBag.OriginalPath = statusCodeResult?.OriginalPath;

        return statusCode switch
        {
            404 => View("NotFound"),
            403 => View("Forbidden"),
            401 => View("Unauthorized"),
            500 => View("ServerError"),
            _ => View("Error")
        };
    }

    [HttpGet]
    [Route("Error")]
    public IActionResult Error()
    {
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        if (exceptionFeature != null)
        {
            _logger.LogError(
                exceptionFeature.Error,
                "Unhandled exception occurred. Path: {Path}",
                exceptionFeature.Path);
        }

        return View();
    }

    [HttpGet]
    public new IActionResult NotFound()
    {
        Response.StatusCode = 404;
        return View();
    }

    [HttpGet]
    public IActionResult Forbidden()
    {
        Response.StatusCode = 403;
        return View();
    }

    [HttpGet]
    public new IActionResult Unauthorized()
    {
        Response.StatusCode = 401;
        return View();
    }

    [HttpGet]
    public IActionResult ServerError()
    {
        Response.StatusCode = 500;
        return View();
    }
}
