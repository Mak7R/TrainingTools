using Contracts.Client.Models;
using Contracts.Client.Services;
using IHttpContextAccessor = Microsoft.AspNetCore.Http.IHttpContextAccessor;
using Microsoft.JSInterop;

namespace Services.Client;

public class BrowserCookiesProvider : ICookiesProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJSRuntime _jsRuntime;

    public BrowserCookiesProvider(IHttpContextAccessor httpContextAccessor, IJSRuntime jsRuntime)
    {
        _httpContextAccessor = httpContextAccessor;
        _jsRuntime = jsRuntime;
    }
    
    public async Task ToRequest(HttpRequest request)
    {
        var cookies = _httpContextAccessor.HttpContext?.Request.Cookies;
        if (cookies != null)
        {
            request.Headers.Add("Cookie", cookies.Select(cookie => $"{cookie.Key}={cookie.Value}").ToArray());
        }
    }

    public async Task FromResponse(HttpResponse response)
    {
        var responseCookies = response.Headers.FirstOrDefault(h => h.Key == "Set-Cookie");
        if (responseCookies.Key != null)
        {
            foreach (var cookie in responseCookies.Value)
            {
                await _jsRuntime.InvokeVoidAsync("setCookie", cookie);
            }
        }
    }
}