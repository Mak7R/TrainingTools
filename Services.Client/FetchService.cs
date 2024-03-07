using Contracts.Client.Models;
using Contracts.Client.Services;

namespace Services.Client;

public class FetchService : IFetchService
{
    private readonly IHttpClientFactory _clientFactory;

    public FetchService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }
    
    public async Task<HttpResponse> Fetch(HttpRequest request)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Parse(request.Method), request.Path);
        requestMessage.Content = new StringContent(request.Content, request.Encoding, request.ContentType);

        foreach (var header in request.Headers) 
            requestMessage.Headers.Add(header.Key, header.Value);

        using var client = _clientFactory.CreateClient();
        var response = await client.SendAsync(requestMessage);
        
        return new HttpResponse(
            isSuccessStatusCode: response.IsSuccessStatusCode,
            statusCode: (int)response.StatusCode,
            content: await response.Content.ReadAsStringAsync(),
            headers: response.Headers
            );
    }
}

