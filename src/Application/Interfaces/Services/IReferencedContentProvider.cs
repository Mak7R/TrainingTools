namespace Application.Interfaces.Services;

public interface IReferencedContentProvider
{
    public Task<string> ParseContentAsync(string? content);
}