using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CommunityCar.Web.Filters;

public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            var isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (isAjax)
            {
                context.Result = new JsonResult(new
                {
                    success = false,
                    message = "Validation failed",
                    errors = errors
                })
                {
                    StatusCode = 400
                };
            }
            else
            {
                // Add errors to TempData for display
                if (context.HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory)) 
                    is Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory factory)
                {
                    var tempData = factory.GetTempData(context.HttpContext);
                    tempData["ErrorToast"] = string.Join(", ", errors);
                }

                // Return to the same view with validation errors
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }
}
