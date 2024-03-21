using System.Text;
using Contracts.Client.Services;
using Microsoft.Extensions.Configuration;

namespace Services.Client;

public class ConfigLinkGenerator : ILinkGenerator
{
    private string Domain { get; }
    private readonly List<(string action, string controller, string url)> _links;

    public ConfigLinkGenerator(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        Domain = configuration["WebLinks:Domain"] ?? throw new  NullReferenceException("Configuration Empty: WebLinks:Domain");
        _links = new List<(string action, string controller, string url)>();
        
        var linksSection = configuration.GetSection("WebLinks:Links") ?? throw new  NullReferenceException("Configuration Empty: WebLinks:Links");
        foreach (var controllerSection in linksSection.GetChildren())
        {
            var controllerName = controllerSection.Key;
            var actionSections = controllerSection.GetChildren();
            _links.AddRange(actionSections.Select(x => (x.Key, controllerName, x.Value ?? string.Empty)));
        }
    }

    private string FindUrl(string action, string controller)
    {
        var pathInfo = _links.FirstOrDefault(path =>
            path.action.Equals(action, StringComparison.CurrentCultureIgnoreCase) &&
            path.controller.Equals(controller, StringComparison.CurrentCultureIgnoreCase));

        if (pathInfo == default)
            throw new NullReferenceException("Url was not found");

        return pathInfo.url;
    }

    private string InjectRouteValues(string url, object? routeValues = null)
    {
        if (routeValues != null)
        {
            var properties = routeValues.GetType().GetProperties();
            var queryString = new StringBuilder();
            foreach (var property in properties)
            {
                var value = property.GetValue(routeValues)?.ToString();
                var token = $"{{{property.Name}}}";
                if (url.Contains(token, StringComparison.CurrentCultureIgnoreCase))
                    url = url.Replace(token, value, StringComparison.CurrentCultureIgnoreCase);
                else
                    queryString.Append($"{property.Name}={value}&");
            }
            if (queryString.Length > 0)
            {
                queryString.Remove(queryString.Length - 1, 1); // Remove the trailing "&"
                url += $"?{queryString}";
            }
        }

        return url;
    }
    
    public string GetPathByAction(string action, string controller, object? routeValues = null)
    {
        var url = Domain + FindUrl(action, controller);
        return InjectRouteValues(url, routeValues);
    }
    
    public string GetLocalPathByAction(string action, string controller, object? routeValues = null)
    {
        var url = FindUrl(action, controller);
        return InjectRouteValues(url, routeValues);
    }
}