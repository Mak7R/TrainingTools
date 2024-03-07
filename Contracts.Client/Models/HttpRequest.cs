using System.Text;

namespace Contracts.Client.Models;

public class HttpRequest
{
    public string Path { get; set; }
    public string Method { get; set; } = "GET";
    public string Content { get; set; } = string.Empty;

    public Dictionary<string, IEnumerable<string?>> Headers { get; } = new();
    public string ContentType { get; set; } = "application/json";
    public Encoding? Encoding { get; set; } = null;

    public HttpRequest(string path)
    {
        Path = path;
    }

    public HttpRequest(string path, string method, string content) : this(path)
    {
        Method = method;
        Content = content;
    }

    public HttpRequest(string path, string method, string content, string contentType, Encoding encoding) : this(path, method, content)
    {
        ContentType = contentType;
        Encoding = encoding;
    }
}