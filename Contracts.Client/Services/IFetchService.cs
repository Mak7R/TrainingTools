using Contracts.Client.Models;

namespace Contracts.Client.Services;

public interface IFetchService
{
    Task<HttpResponse> Fetch(HttpRequest request);
}