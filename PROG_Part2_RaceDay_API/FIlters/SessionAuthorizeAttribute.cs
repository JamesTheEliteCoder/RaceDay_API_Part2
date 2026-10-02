using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PROG_Part2_RaceDay_API.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string? _requiredRole;

    public SessionAuthorizeAttribute(string? requiredRole = null)
    {
        _requiredRole = requiredRole;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;

        if (session.GetInt32("UserId") is null)
        {
            context.Result = new UnauthorizedObjectResult(
                new { message = "You must be logged in to access this endpoint." });
            return;
        }

        if (_requiredRole is not null &&
            !string.Equals(
                session.GetString("Role"),
                _requiredRole,
                StringComparison.Ordinal))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
    }
}