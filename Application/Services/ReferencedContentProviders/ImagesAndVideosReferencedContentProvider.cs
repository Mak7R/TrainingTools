using System.Text.RegularExpressions;
using Application.Interfaces.ServiceInterfaces;

namespace Application.Services.ReferencedContentProviders;

public class ImagesAndVideosReferencedContentProvider : IReferencedContentProvider, IDisposable
{
    private readonly HttpClient _httpClient;

    public ImagesAndVideosReferencedContentProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> ParseContentAsync(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }
        
        var urlPattern = @"(http[s]?:\/\/[^\s]+)";
        var regex = new Regex(urlPattern);
        var matches = regex.Matches(content);

        foreach (Match match in matches)
        {
            var url = match.Value;
            string embeddedContent;

            if (url.Contains("youtube.com") || url.Contains("youtu.be"))
            {
                embeddedContent = $"<iframe class='youtube-video' src='{ConvertYoutubeRedToEmbedFormat(url)}' frameborder='0' allow='accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share' referrerpolicy='strict-origin-when-cross-origin' allowfullscreen></iframe>";
            }
            else if (url.Contains("tiktok.com"))
            {
                embeddedContent = $"<blockquote class='tiktok-embed' cite='{url}' data-video-id='{GetTikTokVideoId(url)}' style='max-width: 605px;min-width: 325px;' >" +
                                  $"<section><a target='_blank' title='Watch on TikTok' href='{url}'></a></section></blockquote>";
                if (!_tiktokScriptAdded)
                {
                    embeddedContent += "<script async src='https://www.tiktok.com/embed.js'></script>";
                    _tiktokScriptAdded = true;
                }
            }
            else if (await IsImageUrl(url))
            {
                embeddedContent = $"<img src='{url}' alt='Image' style='max-width: 40%; max-height: 40%;'/>";
            }
            else
            {
                embeddedContent = $"<a href='{url}'>{url}</a>";
            }

            content = content.Replace(url, embeddedContent);
        }

        return content;
    }
    
    private static string ConvertYoutubeRedToEmbedFormat(string url)
    {
        string videoId = "";

        if (url.Contains("embed"))
        {
            videoId = new Uri(url).Segments[2];
        }
        else if (url.Contains("youtu.be"))
        {
            videoId = new Uri(url).Segments[1];
        }
        else if (url.Contains("watch"))
        {
            var query = new Uri(url).Query;
            var match = Regex.Match(query, @"[?&]v=([^&]+)");
            if (match.Success)
            {
                videoId = match.Groups[1].Value;
            }
        }
        else if (url.Contains("shorts"))
        {
            videoId = new Uri(url).Segments[2];
        }

        if (!string.IsNullOrEmpty(videoId))
        {
            return $"https://www.youtube.com/embed/{videoId}";
        }

        return url;
    }


    private static readonly List<string> DefaultImageEndings = new List<string>()
    {
        ".png", ".jpeg", ".gif", ".svg"
    };

    private bool _tiktokScriptAdded = false;

    private async Task<bool> IsImageUrl(string url)
    {
        if (DefaultImageEndings.Any(url.EndsWith)) return true;

        try
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
            response.EnsureSuccessStatusCode();
            var contentType = response.Content.Headers.ContentType?.MediaType;
            return contentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ?? false;
        }
        catch
        {
            return false;
        }
    }

    private string GetTikTokVideoId(string url)
    {
        var videoIdPattern = @"video\/(\d+)";
        var match = Regex.Match(url, videoIdPattern);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }


    public void Dispose()
    {
        _httpClient.Dispose();
    }
}