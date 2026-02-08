using CommunityCar.Domain.Interfaces.Common;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CommunityCar.Web.Filters;

public class UnitOfWorkFilter : IAsyncActionFilter
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnitOfWorkFilter> _logger;

    public UnitOfWorkFilter(IUnitOfWork unitOfWork, ILogger<UnitOfWorkFilter> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Check if this is a modifying operation (POST, PUT, DELETE, PATCH)
        var method = context.HttpContext.Request.Method;
        var isModifyingOperation = method == "POST" || method == "PUT" || method == "DELETE" || method == "PATCH";

        if (!isModifyingOperation)
        {
            // For GET requests, just execute the action without transaction
            await next();
            return;
        }

        try
        {
            // Execute the action
            var executedContext = await next();

            // If no exception occurred and it's a modifying operation, commit the transaction
            if (executedContext.Exception == null)
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogDebug("Unit of Work committed successfully for {Method} {Path}",
                    method, context.HttpContext.Request.Path);
            }
            else
            {
                // If there was an exception, rollback will happen automatically
                _logger.LogWarning("Unit of Work not committed due to exception in {Method} {Path}",
                    method, context.HttpContext.Request.Path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Unit of Work filter for {Method} {Path}",
                method, context.HttpContext.Request.Path);
            throw;
        }
    }
}
