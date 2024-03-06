using Contracts.Services;
using Microsoft.AspNetCore.Http;

namespace SimpleAuthorizer;

public class CookiesSession : ICookiesSession
{
    private readonly ISessionContainer<Guid, Guid> _sessionContainer;
    private const string CookiesKey = "SESSION_ID";

    public CookiesSession(ISessionContainer<Guid, Guid> container)
    {
        _sessionContainer = container;
    }

    public void AddAuthentication(HttpContext context, Guid id)
    {
        RemoveAuthentication(context);
        var sessionId = _sessionContainer.AddAuthentication(id);
        context.Response.Cookies.Append(CookiesKey, sessionId.ToString());
    }

    public void RemoveAuthentication(HttpContext context)
    {
        if (!context.Request.Cookies.ContainsKey(CookiesKey)) return;
        
        if (GetAuthentication(context, out Guid existId))
        {
            _sessionContainer.RemoveAuthentication(existId);
        }
        context.Response.Cookies.Delete(CookiesKey);
    }

    public bool GetAuthentication(HttpContext context, out Guid id)
    {
        var sessionStringId = context.Request.Cookies[CookiesKey];
        if (sessionStringId != null && Guid.TryParse(sessionStringId, out Guid sessionId))
        {
            return _sessionContainer.GetAuthentication(sessionId, out id);
        }
        
        id = default;
        return false;
    }
}