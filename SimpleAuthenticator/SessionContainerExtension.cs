
using Contracts;
using Contracts.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleAuthorizer;

public static class SessionContainerExtension
{
    public static bool AddIdToSession(this HttpContext context, Guid valueId)
    {
        var container = context.RequestServices.GetRequiredService<ISessionContainer<Guid, Guid>>();
        
        if (context.Request.Cookies.ContainsKey(AuthMiddleware.SessionIdKey))
        {
            try
            {
                container.RemoveAuthorization(Guid.Parse(context.Request.Cookies[AuthMiddleware.SessionIdKey]!));
            }
            catch
            {
                return false;
            }
        }
        var sessionId = container.AddAuthorization(valueId);
        context.Response.Cookies.Append(AuthMiddleware.SessionIdKey, sessionId.ToString());
        return true;
    }

    public static Guid? GetIdFromSession(this HttpContext httpContext) => (Guid?)httpContext.Items[AuthMiddleware.HttpContextItemsKey];
}