using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Presentation.Filters;

public class CustomAuthorizeAttribute : ActionFilterAttribute
{
    private readonly bool _requireAuth;
    private readonly string[] _allowedRoles;

    public CustomAuthorizeAttribute(bool requireAuth = true, params string[] allowedRoles)
    {
        _requireAuth = requireAuth;
        _allowedRoles = allowedRoles;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // 1. Authentication not required
        if (!_requireAuth)
        {
            base.OnActionExecuting(context);
            return;
        }

        var user = context.HttpContext.User;

        // 2. Authentication required but not authenticated
        if (user?.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedObjectResult(ApiResponse<string>.ErrorResponse("You need to log in to perform this action."));
            return;
        }

        // 3. Check Role
        if (_allowedRoles != null && _allowedRoles.Length > 0)
        {
            // Check InRole or find Claim directly, case-insensitive (in case database stores uppercase)
            bool hasRole = _allowedRoles.Any(role => 
                user.IsInRole(role) || 
                user.HasClaim(c => 
                    (c.Type == ClaimTypes.Role || c.Type == "role") && 
                    c.Value.Equals(role, StringComparison.OrdinalIgnoreCase)
                )
            );
            
            if (!hasRole)
            {
                // DEBUG: Get all existing claims in Token to print to log and find error
                var allClaims = string.Join(" | ", user.Claims.Select(c => $"{c.Type}:{c.Value}"));
                System.Console.WriteLine($"[AUTH FAILED] User is missing required roles: {string.Join(",", _allowedRoles)}");
                System.Console.WriteLine($"[AUTH FAILED] User Claims: {allClaims}");

                context.Result = new ObjectResult(ApiResponse<string>.ErrorResponse(
                    $"You do not have permission to access. Debug Claims: {allClaims}"))
                {
                    StatusCode = 403
                };
                return;
            }
        }

        base.OnActionExecuting(context);
    }
}
