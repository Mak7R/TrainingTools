using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace WebUI.Middlewares;

public class SetCultureMiddleware
{
    private readonly RequestDelegate _next;
    public static string DefaultCulture = "en-US";

    public SetCultureMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(CookieRequestCultureProvider.DefaultCookieName, out var culture))
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        }
        else
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(DefaultCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(DefaultCulture);
        }
        await _next(context);
    }
}

public static class SetCultureMiddlewareExtensions
{
    public static IApplicationBuilder UseSetCultureMiddleware(this IApplicationBuilder builder, string defaultCulture = "en-US")
    {
        SetCultureMiddleware.DefaultCulture = defaultCulture;
        return builder.UseMiddleware<SetCultureMiddleware>();
    }
}