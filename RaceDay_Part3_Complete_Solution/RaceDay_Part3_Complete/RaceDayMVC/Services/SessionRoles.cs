using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDayMVC.Services;

public static class SessionKeys
{
    public const string Token = "RaceDay.AccessToken";
    public const string UserId = "RaceDay.UserId";
    public const string Role = "RaceDay.Role";
    public const string Email = "RaceDay.Email";
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireSessionRoleAttribute : Attribute, IAsyncActionFilter
{
    private readonly string[] _roles;
    public RequireSessionRoleAttribute(params string[] roles) => _roles = roles;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var session = context.HttpContext.Session;
        if (string.IsNullOrWhiteSpace(session.GetString(SessionKeys.Token)))
        {
            string returnUrl = context.HttpContext.Request.Path.ToString() + context.HttpContext.Request.QueryString.ToString();
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
            return;
        }
        if (_roles.Length > 0 && !_roles.Contains(session.GetString(SessionKeys.Role), StringComparer.Ordinal))
        {
            context.Result = new RedirectToActionResult("Denied", "Account", null);
            return;
        }
        await next();
    }
}
