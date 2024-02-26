using Contracts;
using Contracts.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SimpleAuthorizer;

public class AuthMiddleware
{
    public static string SessionIdKey { get; set; } = "SessionIdKey";
    public static object HttpContextItemsKey { get; set; } = "userId";

    private readonly RequestDelegate _next;
    private readonly ISessionContainer<Guid, Guid> _sessionContainer;

    public AuthMiddleware(RequestDelegate next, ISessionContainer<Guid, Guid> sessionContainer)
    {
        _next = next;
        _sessionContainer = sessionContainer;
    }
    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.ContainsKey(SessionIdKey))
        {
            var strGuid = httpContext.Request.Cookies[SessionIdKey]!;

            Guid guid;
            try
            {
                guid = Guid.Parse(strGuid);
            }
            catch (FormatException)
            {
                await _next(httpContext);
                return;
            }

            if (_sessionContainer.FindAuthorization(guid, out var userId))
            {
                httpContext.Items.Add(HttpContextItemsKey, userId);
            }
        }
            
        await _next(httpContext);
    }
}

// Extension method used to add the middleware to the HTTP request pipeline.
public static class AuthMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthMiddleware>();
    }
}