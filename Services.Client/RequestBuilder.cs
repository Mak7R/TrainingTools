using System.Text;
using System.Text.Json;
using Contracts.Client.Models;
using Contracts.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Services.Client;

public class RequestBuilder
{
    private readonly IServiceProvider _serviceProvider;
    private HttpRequest? _request;
    private readonly List<Task> _tasks = [];
    
    private HttpRequest Request
    {
        get
        {
            if (_request == null) throw new NullReferenceException("Request was null");
            return _request;
        }
        set => _request = value ?? throw new NullReferenceException();
    }
    
    
    public RequestBuilder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public RequestBuilder RequestToUrl(string url)
    {
        Request = new HttpRequest(url);
        return this;
    }

    public RequestBuilder RequestToAction(string action, string controller, object? routeValues = null, bool isLocal = false)
    {
        var linkGenerator = _serviceProvider.GetRequiredService<ILinkGenerator>();
        
        var url = isLocal ? 
            linkGenerator.GetLocalPathByAction(action, controller, routeValues) : 
            linkGenerator.GetPathByAction(action, controller, routeValues);

        Request = new HttpRequest(url);
        return this;
    }
    
    public RequestBuilder WithContent(string content, Encoding? encoding = null, string? contentType = null)
    {
        Request.Content = content;
        Request.Encoding = encoding;
        if (contentType != null) Request.ContentType = contentType;
        return this;
    }

    public RequestBuilder WithContent(object obj)
    {
        Request.Content = JsonSerializer.Serialize(obj);
        Request.Encoding = Encoding.UTF8;
        Request.ContentType = "application/json";
        return this;
    }

    public RequestBuilder WithMethod(string method)
    {
        Request.Method = method;
        return this;
    }

    public RequestBuilder WithMethod(HttpMethod method)
    {
        Request.Method = method.ToString();
        return this;
    }

    public RequestBuilder WithHeader(string key, IEnumerable<string> values)
    {
        Request.Headers.Add(key, values);
        return this;
    }

    public RequestBuilder WithHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        foreach (var header in headers) Request.Headers.Add(header.Key, header.Value);
        return this;
    }
    
    public RequestBuilder WithCookies()
    {
        var cookiesProvider = _serviceProvider.GetRequiredService<ICookiesProvider>();
        var task = cookiesProvider.ToRequest(Request);
        _tasks.Add(task);
        return this;
    }

    public async Task<ResponseDestructor> Fetch(Action<Exception>? exceptionHandler = null)
    {
        if (_tasks.Count != 0) 
            foreach (var task in _tasks) 
                await task;

        var fetchService = _serviceProvider.GetRequiredService<IFetchService>();
        HttpResponse response;
        try
        {
            response = await fetchService.Fetch(Request);
        }
        catch (Exception e)
        {
            exceptionHandler?.Invoke(e);
            response = new HttpResponse(false, 0, e.Message, new List<KeyValuePair<string, IEnumerable<string>>>());
        }
        
        return new ResponseDestructor(_serviceProvider, response);
    }
}

public class ResponseDestructor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpResponse _response;
    private bool _isResponseHandled = false;
    private Func<HttpResponse, Task>? _asyncHandler;
    private List<Task> _tasks = [];

    public ResponseDestructor(IServiceProvider serviceProvider, HttpResponse response)
    {
        _serviceProvider = serviceProvider;
        _response = response;
    }

    public ResponseDestructor LoadCookies()
    {
        var cookiesProvider = _serviceProvider.GetRequiredService<ICookiesProvider>();
        var task = cookiesProvider.FromResponse(_response);
        _tasks.Add(task);
        return this;
    }

    public ResponseDestructor OnStatusCode(int statusCode, Action<HttpResponse> handler)
    {
        if (_response.StatusCode == statusCode)
        {
            handler(_response);
            _isResponseHandled = true;
        }
        return this;
    }
    
    public ResponseDestructor OnStatusCodeAsync(int statusCode,
        Func<HttpResponse, Task> handler)
    {
        if (_response.StatusCode == statusCode)
        {
            _asyncHandler = handler;
            _isResponseHandled = true;
        }
        return this;
    }

    public ResponseDestructor OnSuccess(Action<HttpResponse> onSuccess)
    {
        if (_response.IsSuccessStatusCode && !_isResponseHandled)
        {
            onSuccess(_response);
            _isResponseHandled = true;
        }
        return this;
    }
    
    public ResponseDestructor OnSuccessAsync(Func<HttpResponse, Task> handler)
    {
        if (_response.IsSuccessStatusCode && !_isResponseHandled)
        {
            _asyncHandler = handler;
            _isResponseHandled = true;
        }
        return this;
    }

    public ResponseDestructor OnUnhandled(Action<HttpResponse> handler)
    {
        if (!_isResponseHandled)
        {
            handler(_response);
        }
        return this;
    }
    
    public ResponseDestructor OnUnhandledAsync(Func<HttpResponse, Task> handler)
    {
        if (!_isResponseHandled)
        {
            _asyncHandler = handler;
        }
        return this;
    }

    public async Task WaitForHandleFinishedAsync()
    {
        if (_tasks.Count != 0) 
            foreach (var task in _tasks) 
                await task;

        if (_asyncHandler != null) await _asyncHandler(_response);
    }
} 