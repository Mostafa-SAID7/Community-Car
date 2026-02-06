using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CommunityCar.Web.Controllers;

public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;
    private readonly IStringLocalizer<ErrorController> _localizer;

    public ErrorController(ILogger<ErrorController> logger, IStringLocalizer<ErrorController> localizer)
    {
        _logger = logger;
        _localizer = localizer;
    }

    [Route("Error/{statusCode}")]
    public IActionResult Index(int statusCode)
    {
        _logger.LogWarning("Error {StatusCode} occurred for path: {OriginalPath}", 
            statusCode, 
            HttpContext.Items["OriginalPath"]);

        ViewBag.StatusCode = statusCode;
        ViewBag.OriginalPath = HttpContext.Items["OriginalPath"];

        return statusCode switch
        {
            400 => View("400"),
            401 => View("401"),
            403 => View("403"),
            404 => View("404"),
            500 => View("500"),
            503 => View("503"),
            _ => View("Index")
        };
    }

    [Route("Error/400")]
    public IActionResult Error400()
    {
        ViewBag.StatusCode = 400;
        ViewBag.Title = _localizer["Error400Title"];
        ViewBag.Message = _localizer["Error400Message"];
        return View("400");
    }

    [Route("Error/401")]
    public IActionResult Error401()
    {
        ViewBag.StatusCode = 401;
        ViewBag.Title = _localizer["Error401Title"];
        ViewBag.Message = _localizer["Error401Message"];
        return View("401");
    }

    [Route("Error/403")]
    public IActionResult Error403()
    {
        ViewBag.StatusCode = 403;
        ViewBag.Title = _localizer["Error403Title"];
        ViewBag.Message = _localizer["Error403Message"];
        return View("403");
    }

    [Route("Error/404")]
    public IActionResult Error404()
    {
        ViewBag.StatusCode = 404;
        ViewBag.Title = _localizer["Error404Title"];
        ViewBag.Message = _localizer["Error404Message"];
        return View("404");
    }

    [Route("Error/500")]
    public IActionResult Error500()
    {
        ViewBag.StatusCode = 500;
        ViewBag.Title = _localizer["Error500Title"];
        ViewBag.Message = _localizer["Error500Message"];
        return View("500");
    }

    [Route("Error/503")]
    public IActionResult Error503()
    {
        ViewBag.StatusCode = 503;
        ViewBag.Title = _localizer["Error503Title"];
        ViewBag.Message = _localizer["Error503Message"];
        return View("503");
    }
}
