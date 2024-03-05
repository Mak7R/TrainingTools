namespace Contracts.Models;

public interface IFilter
{
    public string? FilterBy { get; }
    public string? FilterValue { get; }
}