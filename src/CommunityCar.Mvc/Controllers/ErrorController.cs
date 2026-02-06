using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CommunityCar.Mvc.Controllers;

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

        // Capture exception details if available
        var exceptionFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (exceptionFeature != null)
        {
            ViewBag.Exception = exceptionFeature.Error;
        }
        else if (HttpContext.Items.ContainsKey("Exception"))
        {
            ViewBag.Exception = HttpContext.Items["Exception"] as Exception;
        }

        var model = new CommunityCar.Web.Models.ErrorViewModel
        {
            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };

        return statusCode switch
        {
            400 => View("400", model),
            401 => View("401", model),
            403 => View("403", model),
            404 => View("404", model),
            500 => View("500", model),
            503 => View("503", model),
            _ => View("Error", model)
        };
    }

    [Route("Error/400")]
    public IActionResult Error400()
    {
        ViewBag.StatusCode = 400;
        ViewBag.Title = _localizer["Error400Title"];
        ViewBag.Message = _localizer["Error400Message"];
        var model = new CommunityCar.Web.Models.ErrorViewModel
        {
            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };
        return View("400", model);
    }

    [Route("Error/401")]
    public IActionResult Error401()
    {
        ViewBag.StatusCode = 401;
        ViewBag.Title = _localizer["Error401Title"];
        ViewBag.Message = _localizer["Error401Message"];
        var model = new CommunityCar.Web.Models.ErrorViewModel
        {
            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };
        return View("401", model);
    }

    [Route("Error/403")]
    public IActionResult Error403()
    {
        ViewBag.StatusCode = 403;
        ViewBag.Title = _localizer["Error403Title"];
        ViewBag.Message = _localizer["Error403Message"];
        var model = new CommunityCar.Web.Models.ErrorViewModel
        {
            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };
        return View("403", model);
    }

    [Route("Error/404")]
    public IActionResult Error404()
    {
        ViewBag.StatusCode = 404;
        ViewBag.Title = _localizer["Error404Title"];
        ViewBag.Message = _localizer["Error404Message"];
        var model = new CommunityCar.Web.Models.ErrorViewModel
        {
            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };
        return View("404", model);
    }

    [Route("Error/500")]
    public IActionResult Error500()
    {
        ViewBag.StatusCode = 500;
        ViewBag.Title = _localizer["Error500Title"];
        ViewBag.Message = _localizer["Error500Message"];

        var exceptionFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (exceptionFeature != null)
        {
            ViewBag.Exception = exceptionFeature.Error;
        }

        var model = new CommunityCar.Web.Models.ErrorViewModel
        {
            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };
        return View("500", model);
    }

    [Route("Error/503")]
    public IActionResult Error503()
    {
        ViewBag.StatusCode = 503;
        ViewBag.Title = _localizer["Error503Title"];
        ViewBag.Message = _localizer["Error503Message"];
        var model = new CommunityCar.Web.Models.ErrorViewModel
        {
            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };
        return View("503", model);
    }
}
