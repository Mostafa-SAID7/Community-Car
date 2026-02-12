using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Mvc.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "A global exception occurred.");

        var isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (isAjax)
        {
            context.Result = new JsonResult(new { success = false, message = context.Exception.Message })
            {
                StatusCode = 500
            };
        }
        else
        {
            if (context.HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory)) is Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory factory)
            {
                var tempData = factory.GetTempData(context.HttpContext);
                tempData["ErrorToast"] = context.Exception.Message;
            }

            var referer = context.HttpContext.Request.Headers["Referer"].ToString();
            if (context.HttpContext.Request.Method == "POST" && !string.IsNullOrEmpty(referer))
            {
                context.Result = new RedirectResult(referer);
            }
            else
            {
                context.Result = new ViewResult { ViewName = "Error" };
            }
        }

        context.ExceptionHandled = true;
    }
}
