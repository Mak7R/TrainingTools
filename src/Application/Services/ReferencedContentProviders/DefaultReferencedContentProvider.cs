using Application.Interfaces.ServiceInterfaces;

namespace Application.Services.ReferencedContentProviders;

public class DefaultReferencedContentProvider : IReferencedContentProvider
{
    public Task<string> ParseContentAsync(string? content)
    {
        return Task.FromResult(content ?? string.Empty);
    }
}