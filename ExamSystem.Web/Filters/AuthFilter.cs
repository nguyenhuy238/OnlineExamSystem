using ExamSystem.Web.Attributes;
using ExamSystem.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ExamSystem.Web.Filters;

public class AuthFilter : IActionFilter, IAsyncPageFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (ShouldSkip(context.ActionDescriptor.EndpointMetadata))
        {
            return;
        }

        if (TryBuildAuthResult(context.HttpContext, context.ActionDescriptor.EndpointMetadata, out var result))
        {
            context.Result = result;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (ShouldSkip(context.ActionDescriptor.EndpointMetadata))
        {
            await next();
            return;
        }

        if (TryBuildAuthResult(context.HttpContext, context.ActionDescriptor.EndpointMetadata, out var result))
        {
            context.Result = result;
            return;
        }

        await next();
    }

    private static bool ShouldSkip(IList<object> metadata)
    {
        return metadata.OfType<IAllowAnonymous>().Any();
    }

    private static bool TryBuildAuthResult(
        HttpContext httpContext,
        IList<object> metadata,
        out IActionResult? result)
    {
        var userId = httpContext.Session.GetInt32(SessionKeys.UserId);
        var role = httpContext.Session.GetString(SessionKeys.Role);

        if (userId is null)
        {
            var returnUrl = $"{httpContext.Request.PathBase}{httpContext.Request.Path}{httpContext.Request.QueryString}";
            result = new RedirectToActionResult("Login", "Auth", new { returnUrl });
            return true;
        }

        var requiredRole = metadata.OfType<RequireRoleAttribute>().FirstOrDefault()?.Role;
        if (!string.IsNullOrWhiteSpace(requiredRole)
            && !string.Equals(requiredRole, role, StringComparison.OrdinalIgnoreCase))
        {
            result = new RedirectToActionResult("AccessDenied", "Auth", null);
            return true;
        }

        result = null;
        return false;
    }
}
