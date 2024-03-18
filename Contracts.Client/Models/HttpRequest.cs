using System.Text;

namespace Contracts.Client.Models;

public class HttpRequest
{
    public string Url { get; set; }
    public string Method { get; set; } = "GET";
    public string Content { get; set; } = string.Empty;

    public Dictionary<string, IEnumerable<string?>> Headers { get; } = new();
    public string ContentType { get; set; } = "application/json";
    public Encoding? Encoding { get; set; } = null;

    public HttpRequest(string url)
    {
        Url = url;
    }

    public HttpRequest(string url, string method, string content) : this(url)
    {
        Method = method;
        Content = content;
    }

    public HttpRequest(string url, string method, string content, string contentType, Encoding encoding) : this(url, method, content)
    {
        ContentType = contentType;
        Encoding = encoding;
    }
}