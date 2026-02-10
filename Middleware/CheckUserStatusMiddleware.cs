using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using UserManagement.Data;

namespace UserManagement.Middleware;

public class CheckUserStatusMiddleware
{
    private readonly RequestDelegate _next;

    public CheckUserStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            await SignOutAndRedirect(context);
            return;
        }

        var user = db.Users.FirstOrDefault(u => u.Id.ToString() == userId);

        if (user == null || user.Status == "Blocked")
        {
            await SignOutAndRedirect(context);
            return;
        }

        await _next(context);
    }

    private static async Task SignOutAndRedirect(HttpContext context)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        context.Response.Redirect("/Account/Login?blocked=true");
    }
}
