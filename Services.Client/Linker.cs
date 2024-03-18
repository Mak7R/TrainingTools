using Contracts.Client.Services;
using Microsoft.AspNetCore.Components;

namespace Services.Client;

public class Linker : ILinkGenerator
{
    private readonly ILinkGenerator _linkGenerator;
    public RequestBuilder RequestBuilder { get; }
    public NavigationManager NavigationManager { get; }

    public Linker(RequestBuilder requestBuilder, ILinkGenerator linkGenerator, NavigationManager navigationManager)
    {
        RequestBuilder = requestBuilder;
        _linkGenerator = linkGenerator;
        NavigationManager = navigationManager;
    }

    public void NavigateToAction(string action, string controller = "Blazor", object? routeValues = null, bool isLocal = true,
        bool forceLoad = true)
    {
        var url = isLocal
            ? _linkGenerator.GetLocalPathByAction(action, controller, routeValues)
            : _linkGenerator.GetPathByAction(action, controller, routeValues);
        
        NavigationManager.NavigateTo(url, forceLoad);
    }


    public string GetPathByAction(string action, string controller, object? routeValues = null)
    {
        return _linkGenerator.GetPathByAction(action, controller, routeValues);
    }

    public string GetLocalPathByAction(string action, string controller, object? routeValues = null)
    {
        return _linkGenerator.GetLocalPathByAction(action, controller, routeValues);
    }
}