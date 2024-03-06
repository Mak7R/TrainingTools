using Microsoft.AspNetCore.Http;

namespace Contracts.Services;

public interface ICookiesSession
{
    public void AddAuthentication(HttpContext context, Guid id);
    public void RemoveAuthentication(HttpContext context);
    public bool GetAuthentication(HttpContext context, out Guid id);
}