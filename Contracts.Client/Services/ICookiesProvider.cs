using Contracts.Client.Models;

namespace Contracts.Client.Services;

public interface ICookiesProvider
{
    public Task ToRequest(HttpRequest request);

    public Task FromResponse(HttpResponse response);
}