namespace Contracts.Client.Models;

public class HttpResponse(
    bool isSuccessStatusCode,
    int statusCode,
    string content,
    IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
{
    public bool IsSuccessStatusCode { get; } = isSuccessStatusCode;
    public int StatusCode { get; } = statusCode;
    public string Content { get; } = content;
    public IEnumerable<KeyValuePair<string,IEnumerable<string>>> Headers { get; } = headers;
}