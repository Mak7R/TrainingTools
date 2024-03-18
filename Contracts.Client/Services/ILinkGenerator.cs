namespace Contracts.Client.Services;

public interface ILinkGenerator
{
    string GetPathByAction(string action, string controller, object? routeValues = null);
    string GetLocalPathByAction(string action, string controller, object? routeValues = null);
}