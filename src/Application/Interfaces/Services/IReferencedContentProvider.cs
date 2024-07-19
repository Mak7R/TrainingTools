namespace Application.Interfaces.ServiceInterfaces;

public interface IReferencedContentProvider
{
    public Task<string> ParseContentAsync(string? content);
}